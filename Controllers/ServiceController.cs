using Newtonsoft.Json;
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
            TempData["Services"] = JsonConvert.SerializeObject(services);
            return View(services);  // Passing services to the View
        }

        // Action method to get services by department name
        public IActionResult DeptServices(int deptId)
        {
            List<Service>? services = null;

            if (TempData.ContainsKey("Services"))
            {
                services = JsonConvert.DeserializeObject<List<Service>>(TempData["Services"].ToString());
                TempData.Keep();
            }

            services = services?.FindAll(s => s.DeptId == deptId);
            return View("Index", services);
        }

        public async Task<IActionResult> PService(int? id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (id == null)
            {
                return NotFound();
            }

            List<Service>? services = null;

            if (TempData.ContainsKey("Services"))
            {
                services = JsonConvert.DeserializeObject<List<Service>>(TempData["Services"].ToString());
                TempData.Keep();
            }
            var service = services?.FirstOrDefault(s => s.ServiceId == id);
            var appliedServices = await _serviceService.GetMyServicesAsync(user.Id, "All");
            bool alreadyApplied = appliedServices.Any(s => s.ServiceID == id);

            if (alreadyApplied)
            {
                ViewBag.Error = "You have already applied for this service.";
            }

            return View(service);
        }

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

            return View(model);
        }

        // Action method to get user's applied services
        [Authorize]
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
        [Authorize]
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

            return NotFound(); 
        }
    }
}
