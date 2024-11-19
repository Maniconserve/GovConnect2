using System.Diagnostics;
using GovConnect.Models;
using GovConnect.Repository;
using Microsoft.AspNetCore.Mvc;

namespace GovConnect.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private SchemeRepository _schemeRepository;

        public HomeController(ILogger<HomeController> logger, SchemeRepository schemeRepository)
        {
            _logger = logger;
            _schemeRepository = schemeRepository;
        }

        public IActionResult Index()
        {
            return View();
        }
        [HttpGet]
        public IActionResult Schemes()
        {
            return View();
        }
        [HttpGet]
        public IActionResult Search()
        {
            return View();
        }
        [HttpGet]
        public async Task<IActionResult> PScheme(int schemeId) {

            var scheme = await _schemeRepository.GetSchemeByIdAsync(schemeId);

            // If the scheme is not found, return a not found error
            if (scheme == null)
            {
                return NotFound();
            }

            // Pass the scheme data to the view
            return View(scheme);
        }
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
