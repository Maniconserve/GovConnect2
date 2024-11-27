using GovConnect.Models;
using GovConnect.Services;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace GovConnect.Controllers
{
    public class SchemeController : Controller
    {
        private readonly ISchemeService _schemeService;

        // Inject the ISchemeService dependency into the constructor
        public SchemeController(ISchemeService schemeService)
        {
            _schemeService = schemeService;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Search()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Search(Eligibility eligibility)
        {
            var schemes = await _schemeService.GetSchemesByEligibilityAsync(eligibility);
            TempData["Schemes"] = JsonConvert.SerializeObject(schemes);
            return View("Schemes", schemes);
        }

        [HttpGet]
        public IActionResult Schemes()
        {
            return View();
        }

        [HttpGet]
        public IActionResult PScheme(int schemeId)
        {
            List<Scheme>? schemes = null;

            if (TempData.ContainsKey("Schemes"))
            {
                schemes = JsonConvert.DeserializeObject<List<Scheme>>(TempData["Schemes"].ToString());
                TempData.Keep();
            }

            var scheme = schemes?.FirstOrDefault(s => s.SchemeID == schemeId);

            return View(scheme);
        }
    }
}
