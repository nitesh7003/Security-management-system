using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using sociosphere.Data;
using sociosphere.Models;
using System.Globalization;
using System.Text;


namespace sociosphere.Controllers
{
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext db;
        public AdminController(ApplicationDbContext db)
        {
            this.db = db;
        }

        public IActionResult AddFlat()
        {
            return View();
        }

        // POST: Admin/AddFlat
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddFlat(addflat flat)
        {
            if (ModelState.IsValid)
            {
                db.Add(flat);
                await db.SaveChangesAsync();
                //return RedirectToAction(nameof(FlatSuccess)); // Redirect to success page or list view
            }
            return View(flat);
        }

        // --------------- Allote Flat -----------------------

        public async Task<IActionResult> AlloteFlat()
        {
            var wingNames = await db.addflats.Select(f => f.wingname).Distinct().ToListAsync();
            var userNames = await db.userregs.Select(u => u.name).ToListAsync();

            ViewBag.WingNames = new SelectList(wingNames);
            ViewBag.UserNames = new SelectList(userNames);

            return View();
        }

        [HttpGet]
        public async Task<JsonResult> GetFloorNumbers(string wingName)
        {
            var floorNumbers = await db.addflats
                .Where(f => f.wingname == wingName)
                .Select(f => f.floorno)
                .Distinct()
                .OrderBy(f => f)
                .ToListAsync();

            return Json(floorNumbers);
        }

        [HttpGet]
        public async Task<JsonResult> GetFlatNumbers(string wingName, int floorNo)
        {
            var flatNumbers = await db.addflats
                .Where(f => f.wingname == wingName && f.floorno == floorNo)
                .Select(f => new { f.flatno, f.flattype })
                .ToListAsync();

            return Json(flatNumbers);
        }
        [HttpGet]
        public JsonResult GetFlatType(string wingName, string flatNo)
        {
            var flatType = db.addflats
                .Where(f => f.wingname == wingName && f.flatno == flatNo)
                .Select(f => new { flattype = f.flattype })
                .FirstOrDefault();

            return Json(flatType);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AlloteFlat(alloteflat model)
        {
            if (ModelState.IsValid)
            {
                model.allotdate = DateTime.Now;
                db.Add(model);
                await db.SaveChangesAsync();
                return RedirectToAction(nameof(AlloteFlat)); // Or wherever you want to redirect after success
            }
            return View(model);
        }



        //  -------------------- Complaint -------------------
        public IActionResult ViewComplaints()
        {
            var complaints = db.addcomplaints.ToList();
            return View(complaints);
        }

        [HttpPost]
        public async Task<IActionResult> ResolveComplaint(int id)
        {
            var complaint = await db.addcomplaints.FindAsync(id);
            if (complaint != null)
            {
                complaint.complaintstatus = "Resolved";
                complaint.resolvedate = DateTime.Now;
                await db.SaveChangesAsync();
            }

            return RedirectToAction("ViewComplaints");
        }


        // ----------------- Announcement -------------------

        [HttpGet]
        public IActionResult AddAnnouncement()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddAnnouncement(AnnouncementViewModel model)
        {
            if (ModelState.IsValid)
            {
                var announcement = new announcement
                {
                    Announcement = model.Announcement,
                    AnnounceDate = DateTime.Now
                };

                db.announcements.Add(announcement);
                db.SaveChanges();

                return RedirectToAction("ViewAnnouncements");
            }
            return View(model);
        }

        [HttpGet]
        public IActionResult ViewAnnouncements()
        {
            // Automatically delete announcements older than 24 hours
            var announcements = db.announcements.ToList();
            var expiredAnnouncements = announcements.Where(a => a.AnnounceDate < DateTime.Now.AddHours(-24)).ToList();
            db.announcements.RemoveRange(expiredAnnouncements);
            db.SaveChanges();

            var activeAnnouncements = db.announcements.Where(a => a.AnnounceDate >= DateTime.Now.AddHours(-24)).ToList();
            return View(activeAnnouncements);
        }


        // -------------------- Add Bill -------------------

        [HttpGet]
        public async Task<IActionResult> AddBill()
        {
            ViewBag.WingNames = new SelectList(await db.alloteflats.Select(x => x.wingname).Distinct().ToListAsync(), "WingName");
            ViewBag.FlatNos = new SelectList(Enumerable.Empty<int>(), "FlatNo");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddBill(billmanagement bill)
        {
            if (ModelState.IsValid)
            {
                bill.BillReleaseDt = DateTime.Now;
                db.billmanagements.Add(bill);
                await db.SaveChangesAsync();
                return RedirectToAction("AddBill");
            }

            ViewBag.WingNames = new SelectList(await db.alloteflats.Select(x => x.wingname).Distinct().ToListAsync(), "WingName");
            ViewBag.FlatNos = new SelectList(Enumerable.Empty<int>(), "FlatNo");
            return View(bill);
        }

        [HttpGet]
        public async Task<JsonResult> GetFlatsByWing(string wingName)
        {
            var flats = await db.alloteflats
                .Where(x => x.wingname == wingName)
                .Select(x => x.flatno)
                .ToListAsync();

            return Json(new SelectList(flats));
        }

        ///      ---------------- REPORT ----------------- 

        [HttpGet]
        public IActionResult GenerateReport()
        {
            ViewBag.ReportTypes = new SelectList(new List<string> { "Bill", "Complaint", "Visitor" });
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> GenerateReport(ReportViewModel model)
        {
            if (ModelState.IsValid)
            {
                if (model.ReportFor == "Bill")
                {
                    var bills = await db.billmanagements
                        .Where(b => b.BillReleaseDt >= model.StartDate && b.BillReleaseDt <= model.EndDate)
                        .ToListAsync();
                    return View("ReportResultsBill", bills);
                }
                else if (model.ReportFor == "Complaint")
                {
                    var complaints = await db.addcomplaints
                        .Where(c => c.raisedate >= model.StartDate && c.raisedate <= model.EndDate)
                        .ToListAsync();
                    return View("ReportResultsComplaint", complaints);
                }
                else if (model.ReportFor == "Visitor")
                {
                    var visitors = await db.gatemanagements
                        .Where(v => v.InDateTime >= model.StartDate && v.InDateTime <= model.EndDate)
                        .ToListAsync();
                    return View("ReportResultsVisitor", visitors);
                }
            }

            ViewBag.ReportTypes = new SelectList(new List<string> { "Bill", "Complaint", "Visitor" });
            return View(model);
        }

        [HttpPost]
        public IActionResult ExportReportToCSV(string reportType)
        {
            var csv = new StringBuilder();

            if (reportType == "Bill")
            {
                var bills = db.billmanagements.ToList();
                csv.AppendLine("Bill Title,Flat Number,Bill Amount,Month,Paid Date,Status");

                foreach (var bill in bills)
                {
                    string formattedMonth = bill.Month.ToString("MMMM yyyy");
                    string formattedAmount = bill.AmountPay.ToString("F2", CultureInfo.InvariantCulture);
                    string formattedPaidDate = bill.BillSbmtDate?.ToString("dd-MM-yyyy");

                    csv.AppendLine($"{bill.Title},{bill.FlatNo},{formattedAmount},{formattedMonth},{formattedPaidDate},{bill.PaidStatus}");
                }
            }
            else if (reportType == "Complaint")
            {
                var complaints = db.addcomplaints.ToList();
                csv.AppendLine("Name,Flat Number,Complaint,Status,Raised Date,Resolved Date");

                foreach (var complaint in complaints)
                {
                    string formattedRaiseDate = complaint.raisedate.ToString("dd-MM-yyyy");
                    string formattedResolveDate = complaint.resolvedate?.ToString("dd-MM-yyyy");

                    csv.AppendLine($"{complaint.name},{complaint.flatno},{complaint.WriteComplaint},{complaint.complaintstatus},{formattedRaiseDate},{formattedResolveDate}");
                }
            }
            else if (reportType == "Visitor")
            {
                var visitors = db.gatemanagements.ToList();
                csv.AppendLine("Visitor Name,Flat Number,Wing Name,Phone,In DateTime,Out DateTime,Status");

                foreach (var visitor in visitors)
                {
                    string formattedInDateTime = visitor.InDateTime.ToString("dd-MM-yyyy HH:mm");
                    string formattedOutDateTime = visitor.OutDateTime?.ToString("dd-MM-yyyy HH:mm");

                    csv.AppendLine($"{visitor.VisitorName},{visitor.FlatNo},{visitor.WingName},{visitor.Phone},{formattedInDateTime},{formattedOutDateTime},{visitor.Status}");
                }
            }

            var fileBytes = Encoding.UTF8.GetBytes(csv.ToString());
            return File(fileBytes, "text/csv", $"{reportType}_Report.csv");
        }



    }
}
