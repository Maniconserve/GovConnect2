using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GovConnect.Data;
using Microsoft.AspNetCore.Identity;
using GovConnect.Models;

namespace GovConnect.Controllers
{
    public class ServiceController : Controller
    {
        private readonly SqlServerDbContext _context; // Your DbContext
        private readonly UserManager<Citizen> _userManager;

        public ServiceController(SqlServerDbContext context, UserManager<Citizen> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // Action method to retrieve all services from the database
        public async Task<IActionResult> Index()
        {
            // Fetch all services, including the related department data if needed
            var services = await _context.DServices
                                        .ToListAsync();

            return View(services);  // Passing services to the View
        }

        public async Task<IActionResult> GetDepartmentWithServicesByName(string deptName)
        {
            // Find department by name and eagerly load related services
            var departmentWithServices = await _context.DDepartments
                .Include(d => d.Services)  // Eagerly load related services
                .FirstOrDefaultAsync(d => d.DeptName == deptName);  // Search by DeptName

            if (departmentWithServices == null)
            {
                return NotFound($"Department with name '{deptName}' not found.");
            }

            // Return the department with its related services
            return View("Index", departmentWithServices.Services);
        }

        public IActionResult PService(int? id)
        {
            // Retrieve the service with the given id from the database
            var service = _context.DServices
                .Include(s => s.FeeDetails)  // Make sure to include related FeeDetails if necessary
                .FirstOrDefault(s => s.ServiceId == id);

            // If service not found, return NotFound or a suitable error page
            if (service == null)
            {
                return NotFound();
            }

            // Pass the service to the view
            return View(service);
        }

        [HttpPost]
        public async Task<IActionResult> Apply(ServiceApplication model)
        {
            // Get the logged-in user
            var user = await _userManager.GetUserAsync(User);  // This will fetch the logged-in user

            if (user == null)
            {
                // If no user is logged in, redirect to login page or show an error
                return RedirectToAction("Login", "Account");  // Or handle the error as needed
            }

            // Validate if model state is valid
            if (ModelState.IsValid)
            {
                // Create a new ServiceApplication instance
                var serviceApplication = new ServiceApplication
                {
                    UserID = user.Id,  // Set the UserID dynamically from the logged-in user
                    ServiceID = model.ServiceID,
                    ApplicationDate = model.ApplicationDate,
                    Status = "Pending",  // Default status, can be updated based on logic
                    OfficerID = null     // OfficerID will be null initially
                };

                // Save the new application to the database
                _context.ServiceApplications.Add(serviceApplication);
                await _context.SaveChangesAsync();  // Use async method for better performance

                TempData["SuccessMessage"] = "You have successfully applied for the service!";
                return View(model); // Return the same view
            }

            // If model state is invalid, return to the current page
            return View(model);
        }
        [HttpGet]
        public IActionResult MyServices()
        {
            return View();
        }
    }
}
