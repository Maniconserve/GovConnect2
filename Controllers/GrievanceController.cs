using GovConnect.Data;
using GovConnect.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GovConnect.Controllers
{
    public class GrievanceController : Controller
    {
        private readonly SqlServerDbContext _context;
        public GrievanceController(SqlServerDbContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Lodge()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Lodge(Grievance grievance)
        {
            if (ModelState.IsValid)
            {
                // Save the grievance to the database (example)
                // Assuming you have a DbContext and a Grievances DbSet
                _context.DGrievances.Add(grievance);
                _context.SaveChanges();

                // Redirect to a different action or return a confirmation view
                return RedirectToAction("Index", "Home");  // Redirect after successful form submission
            }

            // If the model is not valid, redisplay the form with error messages
            return View(grievance);
        }

        [HttpGet]
        public IActionResult Status()
        {
            return View();
        }

        [HttpGet]
        public IActionResult MyGrievances() { 
            return View(new List<ServiceApplication>());
        }
    }
}
