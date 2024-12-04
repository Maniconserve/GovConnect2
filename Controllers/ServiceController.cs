using Microsoft.AspNetCore.Authentication.Cookies;
using Newtonsoft.Json;
namespace GovConnect.Controllers
{
    /// <summary>
    /// Controller to manage services available to citizens, including viewing, applying, and withdrawing services.
    /// </summary>
    public class ServiceController : Controller
    {
        private readonly IServiceService _serviceService;
        private readonly UserManager<Citizen> _userManager;

        // Inject dependencies into the constructor
        public ServiceController(IServiceService serviceService, UserManager<Citizen> userManager)
        {
            _serviceService = serviceService;
            _userManager = userManager;
        }

        /// <summary>
        /// Retrieves all services from the database and displays them to the user.
        /// </summary>
        public async Task<IActionResult> Index()
        {
            // Retrieve all services asynchronously
            var services = await _serviceService.GetAllServicesAsync();

            // Store the services in TempData (to pass to another action/view)
            TempData["Services"] = JsonConvert.SerializeObject(services);

            // Return the view with the services
            return View(services);
        }

        /// <summary>
        /// Retrieves and filters services based on the department ID.
        /// </summary>
        /// <param name="deptId">The department ID to filter services by.</param>
        public IActionResult DeptServices(int deptId)
        {
            List<Service>? services = null;

            // Check if the services are available in TempData
            if (TempData.ContainsKey("Services"))
            {
                // Deserialize the list of services from TempData
                services = JsonConvert.DeserializeObject<List<Service>>(TempData["Services"].ToString());
                // Keep TempData for further use in other actions
                TempData.Keep();
            }

            // Filter the services by department ID
            services = services?.FindAll(s => s.DeptId == deptId);

            // Return the filtered services (Index view)
            return View("Index", services);
        }

        /// <summary>
        /// Retrieves and displays a specific service's details and checks if the user has already applied for it.
        /// </summary>
        /// <param name="id">The service ID to display details for.</param>
        [Authorize(Roles = "User", AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme)]
        public async Task<IActionResult> PService(int? id)
        {
            var user = await _userManager.GetUserAsync(User);

            // Return NotFound if the service ID is null
            if (id == null)
            {
                return NotFound();
            }
            if(user == null)
            {
                RedirectToAction("Login","Citizen");
            }
            List<Service>? services = null;

            // Deserialize the services stored in TempData
            if (TempData.ContainsKey("Services"))
            {
                services = JsonConvert.DeserializeObject<List<Service>>(TempData["Services"].ToString());
                TempData.Keep();
            }

            // Find the specific service by its ID
            var service = services?.FirstOrDefault(s => s.ServiceId == id);

            // Check if the user has already applied for this service
            var appliedServices = await _serviceService.GetMyServicesAsync(user.Id, "All");
            bool alreadyApplied = appliedServices.Any(s => s.ServiceID == id);

            // Display a message if the user has already applied
            if (alreadyApplied)
            {
                ViewBag.Error = "You have already applied for this service.";
            }

            // Return the service details view
            return View(service);
        }

        /// <summary>
        /// Allows a user to apply for a service.
        /// </summary>
        /// <param name="model">The service application model containing user input.</param>
        [HttpPost]
        public async Task<IActionResult> Apply(ServiceApplication model)
        {
            var user = await _userManager.GetUserAsync(User);

            // Redirect to login page if user is not authenticated
            if (user == null)
            {
                return RedirectToAction("Login", "Citizen");
            }

            // Apply for the service asynchronously
            bool success = await _serviceService.ApplyForServiceAsync(model, user.Id);

            // If the application is successful, redirect to MyServices
            if (success)
            {
                return RedirectToAction("MyServices");
            }

            // Return the view with the model if the application fails
            return View(model);
        }

        /// <summary>
        /// Retrieves and displays a list of services that the user has applied for.
        /// </summary>
        /// <param name="statusFilter">The status filter (default is "All").</param>
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> MyServices(string statusFilter = "All")
        {
            var user = await _userManager.GetUserAsync(User);

            // Redirect to login page if user is not authenticated
            if (user == null)
            {
                return RedirectToAction("Login", "Citizen");
            }

            // Retrieve the services that the user has applied for based on the status filter
            var appliedServices = await _serviceService.GetMyServicesAsync(user.Id, statusFilter);

            // Return the view with the applied services
            return View(appliedServices);
        }

        /// <summary>
        /// Allows the user to withdraw a previously applied service.
        /// </summary>
        /// <param name="id">The ID of the service to withdraw.</param>
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Withdraw(int id)
        {
            var user = await _userManager.GetUserAsync(User);

            // Redirect to login page if user is not authenticated
            if (user == null)
            {
                return RedirectToAction("Login", "Citizen");
            }

            // Withdraw the service application asynchronously
            bool success = await _serviceService.WithdrawServiceAsync(id, user.Id);

            // If the withdrawal is successful, redirect to MyServices
            if (success)
            {
                return RedirectToAction("MyServices");
            }

            // Return NotFound if the withdrawal was not successful
            return NotFound();
        }
    }
}
