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
            var services = await _context.Services
                                        .ToListAsync();

            return View(services);  // Passing services to the View
        }

        public async Task<IActionResult> GetDepartmentWithServicesByName(string deptName)
        {
            // Find department by name and eagerly load related services
            var departmentWithServices = await _context.Departments
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
            var service = _context.Services
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
            model.Status = "Pending";
            model.UserID = user.Id;
                // Save the new application to the database
            _context.ServiceApplications.Add(model);
            await _context.SaveChangesAsync();  // Use async method for better performance

            return View(); 
        }
        [HttpGet]
        public async Task<IActionResult> MyServices(string statusFilter = "All")  // Default to "In Progress"
        {
            // Get the logged-in user
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                // If no user is logged in, redirect to the login page
                return RedirectToAction("Login", "Account");
            }

            // Query the database for services applied by the logged-in user and filter by status
            var appliedServicesQuery = _context.ServiceApplications
                .Where(s => s.UserID == user.Id);  // Filter by the logged-in user

            // Apply status filter
            if (statusFilter != "All")
            {
                appliedServicesQuery = appliedServicesQuery.Where(s => s.Status == statusFilter);
            }

            var appliedServices = await appliedServicesQuery.ToListAsync();

            // Pass the data to the view
            return View(appliedServices);
        }


        [HttpGet]
        public async Task<IActionResult> Withdraw(int id)
        {
            // Get the logged-in user
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Find the service application by ID
            var serviceApplication = await _context.ServiceApplications
                .FirstOrDefaultAsync(s => s.ApplicationID == id && s.UserID == user.Id); // Ensure only the logged-in user can withdraw their application

            if (serviceApplication == null)
            {
                // If the application is not found or the user doesn't have permission, return an error view or redirect
                return NotFound(); // Or you could redirect to an error page
            }

            // Change the status of the application to "Withdrawn"
            serviceApplication.Status = "Withdrawn";

            // Save the changes to the database
            _context.Update(serviceApplication);
            await _context.SaveChangesAsync();

            return RedirectToAction("MyServices");
        }


    }
}
