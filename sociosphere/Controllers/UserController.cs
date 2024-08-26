using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Razorpay.Api;
using sociosphere.Data;
using sociosphere.Models;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.Net.Mail;
using System.IO;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.EntityFrameworkCore;

namespace sociosphere.Controllers
{
    public class UserController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly RazorpaySettings _razorpaySettings;
        private readonly EmailSettings _emailSettings;

        public UserController(ApplicationDbContext context, IOptions<RazorpaySettings> razorpaySettings, IOptions<EmailSettings> emailSettings)
        {
            _context = context;
            _razorpaySettings = razorpaySettings.Value;
            _emailSettings = emailSettings.Value;
        }

        [HttpGet]
        public IActionResult AddComplaint()
        {
            var userName = HttpContext.Session.GetString("name");
            var userFlat = _context.alloteflats.FirstOrDefault(x => x.name == userName);
            var model = new addcomplaint
            {
                name = userName,
                flatno = userFlat?.flatno ?? 0
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddComplaint(addcomplaint model)
        {
            if (ModelState.IsValid)
            {
                _context.addcomplaints.Add(model);
                _context.SaveChanges();
                return RedirectToAction("ComplaintSuccess");
            }
            return View(model);
        }

        [HttpGet]
        public IActionResult ViewAnnouncements()
        {
            var announcements = _context.announcements.ToList();
            var expiredAnnouncements = announcements.Where(a => a.AnnounceDate < DateTime.Now.AddHours(-24)).ToList();
            _context.announcements.RemoveRange(expiredAnnouncements);
            _context.SaveChanges();
            var activeAnnouncements = _context.announcements.Where(a => a.AnnounceDate >= DateTime.Now.AddHours(-24)).ToList();
            return View(activeAnnouncements);
        }

        [HttpGet]
        public async Task<IActionResult> ViewVisitors()
        {
            string userWingName = HttpContext.Session.GetString("WingName");
            int userFlatNo = int.Parse(HttpContext.Session.GetString("FlatNo"));
            var visitors = await _context.gatemanagements
                .Where(v => v.WingName == userWingName && v.FlatNo == userFlatNo)
                .ToListAsync();
            return View(visitors);
        }

        [HttpGet]
        public IActionResult ViewBills()
        {
            var userName = HttpContext.Session.GetString("name");
            var userFlatDetails = _context.alloteflats.FirstOrDefault(x => x.name == userName);
            if (userFlatDetails != null)
            {
                var userWingName = userFlatDetails.wingname;
                var userFlatNo = userFlatDetails.flatno;
                var userBills = _context.billmanagements
                                        .Where(b => b.WingName == userWingName && b.FlatNo == userFlatNo)
                                        .ToList();
                return View(userBills);
            }
            else
            {
                ModelState.AddModelError(string.Empty, "No flat details found for the user.");
                return View();
            }
        }

        [HttpPost]
        public IActionResult PayBill(int billId)
        {
            var bill = _context.billmanagements.Find(billId);
            if (bill == null || bill.PaidStatus == "Paid")
            {
                return RedirectToAction("ViewBills");
            }

            // Calculate the amount in paise (Razorpay expects the amount in the smallest currency unit)
            var amountInPaise = (int)(bill.AmountPay * 100);

            if (amountInPaise < 100)
            {
                // Handle case where amount is too low
                TempData["Error"] = "Invalid amount. Minimum value is 1 INR.";
                return RedirectToAction("ViewBills");
            }

            // Create Razorpay order
            var client = new RazorpayClient("rzp_test_Kl7588Yie2yJTV", "6dN9Nqs7M6HPFMlL45AhaTgp");
            var options = new Dictionary<string, object>
            {
                { "amount", amountInPaise },
                { "currency", "INR" },
                { "receipt", "rcptid_" + billId }
            };
            Order order = client.Order.Create(options);

            // Store the order ID in the ViewBag to use it in the payment form
            ViewBag.OrderId = order["id"].ToString();
            ViewBag.Amount = bill.AmountPay; // Pass the amount for display in INR
            ViewBag.BillId = billId;

            return View("PaymentPage");
        }


        // PaymentSuccess action: Called by Razorpay on successful payment
        [HttpPost]
        public IActionResult PaymentSuccess(string razorpayPaymentId, string razorpayOrderId, string razorpaySignature, int billId)
        {
            var bill = _context.billmanagements.FirstOrDefault(b => b.Id == billId);
            if (bill != null && bill.PaidStatus == "Pending")
            {
                // Update the bill status to "Paid"
                bill.PaidStatus = "Paid";
                bill.BillSbmtDate = DateTime.Now;
                _context.SaveChanges();

                SendInvoice(bill);

            }

            return RedirectToAction("ViewBills");
        }


        private void SendInvoice(billmanagement bill)
        {
            var userEmail = HttpContext.Session.GetString("Email");

            // Create invoice as a PDF file
            var pdfPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "invoices", $"Invoice_{bill.Id}.pdf");
            GeneratePDF(pdfPath, bill);

            // Send email
            using (MailMessage mail = new MailMessage())
            {
                mail.From = new MailAddress(_emailSettings.SenderEmail);
                mail.To.Add(userEmail);
                mail.Subject = "Your Invoice";
                mail.Body = "Please find your invoice attached.";
                mail.Attachments.Add(new Attachment(pdfPath));

                using (SmtpClient smtp = new SmtpClient(_emailSettings.SmtpServer, _emailSettings.SmtpPort))
                {
                    smtp.Credentials = new System.Net.NetworkCredential(_emailSettings.SenderEmail, _emailSettings.SenderPassword);
                    smtp.EnableSsl = true;
                    smtp.Send(mail);
                }
            }
        }



        //private void GeneratePDF(string pdfPath, billmanagement bill)
        //{
        //    using (FileStream fs = new FileStream(pdfPath, FileMode.Create, FileAccess.Write))
        //    {
        //        using (Document document = new Document(PageSize.A4))
        //        {
        //            PdfWriter.GetInstance(document, fs);
        //            document.Open();

        //            // Add content to the PDF
        //            document.Add(new Paragraph($"Title: {bill.Title}"));
        //            document.Add(new Paragraph($"Amount: {bill.AmountPay:C}"));
        //            document.Add(new Paragraph($"Month: {bill.Month:MMMM yyyy}"));
        //            document.Add(new Paragraph($"Status: {bill.PaidStatus}"));
        //            document.Add(new Paragraph($"Bill Release Date: {bill.BillReleaseDt:dd-MM-yyyy}"));
        //            document.Add(new Paragraph($"Submission Date: {bill.BillSbmtDate:dd-MM-yyyy}"));

        //            document.Close();
        //        }
        //    }
        //}

        private void GeneratePDF(string pdfPath, billmanagement bill)
        {
            using (FileStream fs = new FileStream(pdfPath, FileMode.Create, FileAccess.Write))
            {
                using (Document document = new Document(PageSize.A4, 36, 36, 54, 36)) // Add margins
                {
                    PdfWriter.GetInstance(document, fs);
                    document.Open();

                    // Add a custom font
                    var fontTitle = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 16, BaseColor.BLACK);
                    var fontHeader = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12, BaseColor.BLACK);
                    var fontBody = FontFactory.GetFont(FontFactory.HELVETICA, 10, BaseColor.BLACK);

                    // Add an invoice title
                    Paragraph title = new Paragraph("INVOICE", fontTitle);
                    title.Alignment = Element.ALIGN_CENTER;
                    document.Add(title);

                    // Add some space
                    document.Add(new Paragraph("\n"));

                    // Add a table for bill details
                    PdfPTable table = new PdfPTable(2);
                    table.WidthPercentage = 100;
                    table.SetWidths(new float[] { 1, 2 }); // Set column widths

                    // Add table headers
                    AddCellToHeader(table, "Description", fontHeader);
                    AddCellToHeader(table, "Details", fontHeader);

                    // Add table body
                    AddCellToBody(table, "Title", fontBody);
                    AddCellToBody(table, bill.Title, fontBody);

                    AddCellToBody(table, "Amount", fontBody);
                    AddCellToBody(table, bill.AmountPay.ToString("C"), fontBody);

                    AddCellToBody(table, "Month", fontBody);
                    AddCellToBody(table, bill.Month.ToString("MMMM yyyy"), fontBody);

                    AddCellToBody(table, "Status", fontBody);
                    AddCellToBody(table, bill.PaidStatus, fontBody);

                    AddCellToBody(table, "Bill Release Date", fontBody);
                    AddCellToBody(table, bill.BillReleaseDt.ToString("dd-MM-yyyy"), fontBody);

                    AddCellToBody(table, "Submission Date", fontBody);
                    AddCellToBody(table, bill.BillSbmtDate?.ToString("dd-MM-yyyy") ?? "N/A", fontBody);

                    document.Add(table);

                    // Add some space
                    document.Add(new Paragraph("\n"));

                    // Add footer text
                    Paragraph footer = new Paragraph("Thank you for your payment!", fontBody);
                    footer.Alignment = Element.ALIGN_CENTER;
                    document.Add(footer);

                    document.Close();
                }
            }
        }

        private void AddCellToHeader(PdfPTable table, string text, Font font)
        {
            PdfPCell cell = new PdfPCell(new Phrase(text, font));
            cell.BackgroundColor = BaseColor.LIGHT_GRAY;
            cell.HorizontalAlignment = Element.ALIGN_CENTER;
            cell.Padding = 5;
            table.AddCell(cell);
        }

        private void AddCellToBody(PdfPTable table, string text, Font font)
        {
            PdfPCell cell = new PdfPCell(new Phrase(text, font));
            cell.HorizontalAlignment = Element.ALIGN_LEFT;
            cell.Padding = 5;
            table.AddCell(cell);
        }



    }
}

///////////////////// ///////////////
//[HttpPost]
//public IActionResult PayBill(int billId)
//{
//    // Find the bill by Id
//    var bill = _context.billmanagements.FirstOrDefault(b => b.Id == billId);

//    if (bill != null && bill.PaidStatus == "Pending")
//    {
//        // Update the PaidStatus to "Paid"
//        bill.PaidStatus = "Paid";

//        // Set the Bill Submission Date to the current date
//        bill.BillSbmtDate = DateTime.Now;

//        // Save the changes to the database
//        _context.SaveChanges();

//        // You can also add a success message to the view, if needed
//        TempData["Message"] = "Bill paid successfully!";
//    }
//    else
//    {
//        // If the bill is not found or already paid, add an error message
//        TempData["ErrorMessage"] = "Bill not found or already paid.";
//    }

//    // Redirect to the ViewBills action to show the updated list
//    return RedirectToAction("ViewBills");
//}
/////////////////////////////////////////////

//[HttpPost]
//public IActionResult PayBill(int billId)
//{
//    var bill = _context.billmanagements.Find(billId);
//    if (bill == null || bill.PaidStatus == "Paid")
//    {
//        return RedirectToAction("ViewBills");
//    }

//    var options = new Dictionary<string, object>
//    {
//        { "amount", bill.AmountPay * 100 }, // Amount in paise
//        { "currency", "INR" },
//        { "receipt", "rcptid_" + bill.Id }
//    };

//    var client = new RazorpayClient(_razorpaySettings.KeyId, _razorpaySettings.KeySecret);
//    Order order = client.Order.Create(options);
//    ViewBag.OrderId = order["id"].ToString();
//    ViewBag.Amount = bill.AmountPay * 100; // Set amount to be sent to the view

//    TempData["RazorpayOrderId"] = ViewBag.OrderId;
//    TempData["BillId"] = billId;

//    return View();
//}

//[HttpPost]
//public IActionResult PaymentSuccess(string razorpay_payment_id, string razorpay_order_id, string razorpay_signature)
//{
//    var orderId = TempData["RazorpayOrderId"]?.ToString();
//    var billId = (int)TempData["BillId"];

//    if (orderId != razorpay_order_id)
//    {
//        return RedirectToAction("ViewBills");
//    }

//    //var bill = _context.billmanagements.Find(billId);
//    //if (bill != null)
//    //{
//    //    bill.PaidStatus = "Paid";
//    //    bill.BillSbmtDate = DateTime.Now;
//    //    _context.SaveChanges();

//    //    SendInvoice(bill);
//    //}

//    var bill = _context.billmanagements.Find(billId);
//    if (bill != null)
//    {
//        bill.PaidStatus = "Paid";
//        bill.BillSbmtDate = DateTime.Now;
//        _context.SaveChanges();

//        SendInvoice(bill);
//    }

//    return RedirectToAction("ViewBills");
//}



//private void SendInvoice(billmanagement bill)
//{
//    var userEmail = HttpContext.Session.GetString("Email");

//    // Create invoice as a PDF file
//    var pdfPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "invoices", $"Invoice_{bill.Id}.pdf");
//    GeneratePDF(pdfPath, bill);

//    // Send email
//    using (MailMessage mail = new MailMessage())
//    {
//        mail.From = new MailAddress(_emailSettings.SenderEmail);
//        mail.To.Add(userEmail);
//        mail.Subject = "Your Invoice";
//        mail.Body = "Please find your invoice attached.";
//        mail.Attachments.Add(new Attachment(pdfPath));

//        using (SmtpClient smtp = new SmtpClient(_emailSettings.SmtpServer, _emailSettings.SmtpPort))
//        {
//            smtp.Credentials = new System.Net.NetworkCredential(_emailSettings.SenderEmail, _emailSettings.SenderPassword);
//            smtp.EnableSsl = true;
//            smtp.Send(mail);
//        }
//    }
//}



//private void GeneratePDF(string pdfPath, billmanagement bill)
//{
//    using (FileStream fs = new FileStream(pdfPath, FileMode.Create, FileAccess.Write))
//    {
//        using (Document document = new Document(PageSize.A4))
//        {
//            PdfWriter.GetInstance(document, fs);
//            document.Open();

//            // Add content to the PDF
//            document.Add(new Paragraph($"Title: {bill.Title}"));
//            document.Add(new Paragraph($"Amount: {bill.AmountPay:C}"));
//            document.Add(new Paragraph($"Month: {bill.Month:MMMM yyyy}"));
//            document.Add(new Paragraph($"Status: {bill.PaidStatus}"));
//            document.Add(new Paragraph($"Bill Release Date: {bill.BillReleaseDt:dd-MM-yyyy}"));
//            document.Add(new Paragraph($"Submission Date: {bill.BillSbmtDate:dd-MM-yyyy}"));

//            document.Close();
//        }
//    }
//}






//using Microsoft.AspNetCore.Mvc;
//using Microsoft.EntityFrameworkCore;
//using Razorpay.Api;
//using sociosphere.Data;
//using sociosphere.Models;
//using System.Linq;
//using System.Security.Claims;
//using System.Threading.Tasks;
//using Microsoft.AspNetCore.Http;
//using System.Net.Mail;
//using System.IO;
//using iTextSharp.text;
//using iTextSharp.text.pdf;

//namespace sociosphere.Controllers
//{
//    public class UserController : Controller
//    {
//        private readonly ApplicationDbContext _context;

//        public UserController(ApplicationDbContext context)
//        {
//            _context = context;
//        }

//        [HttpGet]
//        public IActionResult AddComplaint()
//        {
//            var userName = HttpContext.Session.GetString("name");
//            var userFlat = _context.alloteflats.FirstOrDefault(x => x.name == userName);
//            var model = new addcomplaint
//            {
//                name = userName,
//                flatno = userFlat?.flatno ?? 0
//            };
//            return View(model);
//        }

//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        public IActionResult AddComplaint(addcomplaint model)
//        {
//            if (ModelState.IsValid)
//            {
//                _context.addcomplaints.Add(model);
//                _context.SaveChanges();
//                return RedirectToAction("ComplaintSuccess");
//            }
//            return View(model);
//        }

//        [HttpGet]
//        public IActionResult ViewAnnouncements()
//        {
//            var announcements = _context.announcements.ToList();
//            var expiredAnnouncements = announcements.Where(a => a.AnnounceDate < DateTime.Now.AddHours(-24)).ToList();
//            _context.announcements.RemoveRange(expiredAnnouncements);
//            _context.SaveChanges();
//            var activeAnnouncements = _context.announcements.Where(a => a.AnnounceDate >= DateTime.Now.AddHours(-24)).ToList();
//            return View(activeAnnouncements);
//        }

//        [HttpGet]
//        public async Task<IActionResult> ViewVisitors()
//        {
//            string userWingName = HttpContext.Session.GetString("WingName");
//            int userFlatNo = int.Parse(HttpContext.Session.GetString("FlatNo"));
//            var visitors = await _context.gatemanagements
//                .Where(v => v.WingName == userWingName && v.FlatNo == userFlatNo)
//                .ToListAsync();
//            return View(visitors);
//        }

//        [HttpGet]
//        public IActionResult ViewBills()
//        {
//            var userName = HttpContext.Session.GetString("name");
//            var userFlatDetails = _context.alloteflats.FirstOrDefault(x => x.name == userName);
//            if (userFlatDetails != null)
//            {
//                var userWingName = userFlatDetails.wingname;
//                var userFlatNo = userFlatDetails.flatno;
//                var userBills = _context.billmanagements
//                                        .Where(b => b.WingName == userWingName && b.FlatNo == userFlatNo)
//                                        .ToList();
//                return View(userBills);
//            }
//            else
//            {
//                ModelState.AddModelError(string.Empty, "No flat details found for the user.");
//                return View();
//            }
//        }

//        [HttpPost]
//        public IActionResult PayBill(int billId)
//        {
//            var bill = _context.billmanagements.Find(billId);
//            if (bill == null || bill.PaidStatus == "Paid")
//            {
//                return RedirectToAction("ViewBills");
//            }

//            var options = new Dictionary<string, object>
//            {
//                { "amount", bill.AmountPay * 100 }, // Amount in paise
//                { "currency", "INR" },
//                { "receipt", "rcptid_" + bill.Id }
//            };

//            var client = new RazorpayClient("rzp_test_Kl7588Yie2yJTV", "6dN9Nqs7M6HPFMlL45AhaTgp");
//            Order order = client.Order.Create(options);
//            ViewBag.OrderId = order["id"].ToString();
//            ViewBag.Amount = bill.AmountPay * 100; // Set amount to be sent to the view

//            TempData["RazorpayOrderId"] = ViewBag.OrderId;
//            TempData["BillId"] = billId;

//            return View();
//        }

//        [HttpPost]
//        public IActionResult PaymentSuccess(string razorpay_payment_id, string razorpay_order_id, string razorpay_signature)
//        {
//            var orderId = TempData["RazorpayOrderId"]?.ToString();
//            var billId = (int)TempData["BillId"];

//            if (orderId != razorpay_order_id)
//            {
//                return RedirectToAction("ViewBills");
//            }

//            var bill = _context.billmanagements.Find(billId);
//            if (bill != null)
//            {
//                bill.PaidStatus = "Paid";
//                bill.BillSbmtDate = DateTime.Now;
//                _context.SaveChanges();

//                SendInvoice(bill);
//            }

//            return RedirectToAction("ViewBills");
//        }

//        private void SendInvoice(billmanagement bill)
//        {
//            var userEmail = HttpContext.Session.GetString("Email");

//            // Create invoice as a PDF file
//            var pdfPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "invoices", $"Invoice_{bill.Id}.pdf");
//            GeneratePDF(pdfPath, bill);

//            // Send email
//            using (MailMessage mail = new MailMessage())
//            {
//                mail.From = new MailAddress("pruthvirajgavali2001@gmail.com");
//                mail.To.Add(userEmail);
//                mail.Subject = "Your Invoice";
//                mail.Body = "Please find your invoice attached.";
//                mail.Attachments.Add(new Attachment(pdfPath));

//                using (SmtpClient smtp = new SmtpClient("smtp.gmail.com", 587))
//                {
//                    smtp.Credentials = new System.Net.NetworkCredential("pruthvirajgavali2001@gmail.com", "zuoddhlazdzterrq");
//                    smtp.EnableSsl = true;
//                    smtp.Send(mail);
//                }
//            }
//        }

//        private void GeneratePDF(string pdfPath, billmanagement bill)
//        {
//            using (FileStream fs = new FileStream(pdfPath, FileMode.Create, FileAccess.Write))
//            {
//                using (Document document = new Document(PageSize.A4))
//                {
//                    PdfWriter.GetInstance(document, fs);
//                    document.Open();

//                    // Add content to the PDF
//                    document.Add(new Paragraph($"Title: {bill.Title}"));
//                    document.Add(new Paragraph($"Amount: {bill.AmountPay:C}"));
//                    document.Add(new Paragraph($"Month: {bill.Month:MMMM yyyy}"));
//                    document.Add(new Paragraph($"Status: {bill.PaidStatus}"));
//                    document.Add(new Paragraph($"Bill Release Date: {bill.BillReleaseDt:dd-MM-yyyy}"));
//                    document.Add(new Paragraph($"Submission Date: {bill.BillSbmtDate:dd-MM-yyyy}"));

//                    document.Close();
//                }
//            }
//        }
//    }
//}








