using GovConnect.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using GovConnect.Models;

namespace GovConnect.Controllers
{
    public class ServiceController : Controller
    {
        private readonly IServiceService _serviceService;
        private readonly UserManager<Citizen> _userManager;

        public ServiceController(IServiceService serviceService, UserManager<Citizen> userManager)
        {
            _serviceService = serviceService;
            _userManager = userManager;
        }

        // Action method to retrieve all services from the database
        public async Task<IActionResult> Index()
        {
            var services = await _serviceService.GetAllServicesAsync();
            return View(services);  // Passing services to the View
        }

        // Action method to get services by department name
        public async Task<IActionResult> GetDepartmentWithServicesByName(string deptName)
        {
            var services = await _serviceService.GetServicesByDepartmentAsync(deptName);
            return View("Index", services);
        }

        // Action method to view a specific service
        public async Task<IActionResult> PService(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var service = await _serviceService.GetServiceByIdAsync(id.Value);
            if (service == null)
            {
                return NotFound();
            }

            return View(service);
        }

        // Action method to apply for a service
        [HttpPost]
        public async Task<IActionResult> Apply(ServiceApplication model)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return RedirectToAction("Login", "Citizen");
            }

            bool success = await _serviceService.ApplyForServiceAsync(model, user.Id);

            if (success)
            {
                return RedirectToAction("MyServices");
            }

            // Handle failure case, e.g., show an error message
            return View(model);
        }

        // Action method to get user's applied services
        [HttpGet]
        public async Task<IActionResult> MyServices(string statusFilter = "All")
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Citizen");
            }

            var appliedServices = await _serviceService.GetMyServicesAsync(user.Id, statusFilter);
            return View(appliedServices);
        }

        // Action method to withdraw a service application
        [HttpGet]
        public async Task<IActionResult> Withdraw(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Citizen");
            }

            bool success = await _serviceService.WithdrawServiceAsync(id, user.Id);

            if (success)
            {
                return RedirectToAction("MyServices");
            }

            return NotFound();  // Or handle this case differently
        }
    }
}
