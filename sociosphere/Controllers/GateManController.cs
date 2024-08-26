using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using sociosphere.Data;
using sociosphere.Models;

namespace sociosphere.Controllers
{
    public class GateManController : Controller
    {
        private readonly ApplicationDbContext _context;

        public GateManController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult AddVisitor()
        {
            ViewBag.WingNames = _context.alloteflats
                .Select(a => a.wingname)
                .Distinct()
                .ToList();

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddVisitor(gatemanagement visitor)
        {
            if (ModelState.IsValid)
            {
                _context.Add(visitor);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(AddVisitor));
            }

            ViewBag.WingNames = _context.alloteflats
                .Select(a => a.wingname)
                .Distinct()
                .ToList();

            return View(visitor);
           
        }

        // AJAX method to get flat numbers based on the selected wing name
        public JsonResult GetFlatsByWing(string wingName)
        {
            var flats = _context.alloteflats
                .Where(a => a.wingname == wingName)
                .Select(a => a.flatno)
                .ToList();

            return Json(flats);
        }

        [HttpGet]
        public async Task<IActionResult> ViewVisitors()
        {
            var visitors = await _context.gatemanagements
                //.Where(v => v.Status == "In")
                .ToListAsync();

            return View(visitors);
        }

        [HttpPost]
        public async Task<IActionResult> MarkAsOut(int id)
        {
            var visitor = await _context.gatemanagements.FindAsync(id);
            if (visitor != null && visitor.Status == "In")
            {
                visitor.Status = "Out";
                visitor.OutDateTime = DateTime.Now;

                _context.Update(visitor);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("ViewVisitors");
        }
    }
}
