using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GovConnect.Data;

namespace GovConnect.Controllers
{
    public class ServiceController : Controller
    {
        private readonly SqlServerDbContext _context; // Your DbContext

        public ServiceController(SqlServerDbContext context)
        {
            _context = context;
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
            return View("Index",departmentWithServices.Services);
        }

        public IActionResult Apply(int? id)
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
    }
}
