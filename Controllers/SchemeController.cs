using GovConnect.Models;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
namespace GovConnect.Controllers
{
    /// <summary>
    /// Controller for managing schemes, including searching, displaying eligible schemes, and viewing scheme details.
    /// </summary>
    public class SchemeController : Controller
    {
        private readonly ISchemeService _schemeService;

        // Inject the ISchemeService dependency into the constructor
        public SchemeController(ISchemeService schemeService)
        {
            _schemeService = schemeService;
        }

        /// <summary>
        /// Displays the landing page or index view for the scheme section.
        /// </summary>
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// Displays the search page for schemes based on eligibility criteria.
        /// </summary>
        [HttpGet]
        public IActionResult Search()
        {
            return View();
        }

        /// <summary>
        /// Processes the eligibility search form and fetches matching schemes.
        /// </summary>
        /// <param name="eligibility">The eligibility criteria entered by the user.</param>
        [HttpPost]
        public async Task<IActionResult> Search(Eligibility eligibility)
        {
            // Get the schemes that match the eligibility criteria
            var schemes = await _schemeService.GetSchemesByEligibilityAsync(eligibility);

            // Serialize the schemes into JSON and store it in TempData for use in another action
            TempData["Schemes"] = JsonConvert.SerializeObject(schemes);

            // Return the list of schemes found based on the eligibility
            return View("Schemes", schemes);
        }

        /// <summary>
        /// Displays a list of schemes (usually after a search).
        /// </summary>
        [HttpGet]
        public IActionResult Schemes()
        {
            return View();
        }

        /// <summary>
        /// Displays the detailed view of a specific scheme by its ID.
        /// It retrieves the scheme from TempData (which was previously set during the search).
        /// </summary>
        /// <param name="schemeId">The ID of the scheme to display details for.</param>
        [HttpGet]
        public IActionResult PScheme(int schemeId)
        {
            List<Scheme>? schemes = null;

            // Check if TempData contains the serialized list of schemes from the search result
            if (TempData.ContainsKey("Schemes"))
            {
                // Deserialize the schemes from TempData
                schemes = JsonConvert.DeserializeObject<List<Scheme>>(TempData["Schemes"].ToString());
                // Ensure that TempData persists across redirects or other actions
                TempData.Keep();
            }

            // Find the specific scheme by its SchemeID
            var scheme = schemes?.FirstOrDefault(s => s.SchemeID == schemeId);

            // Return the scheme details view, passing the scheme to the view
            return View(scheme);
        }
        [HttpGet]
        public async Task<IActionResult> ProfileScheme(int schemeId, string eligibilityJson)
        {
            var eligibility = JsonConvert.DeserializeObject<Eligibility>(eligibilityJson);
            List<Scheme> schemes = await _schemeService.GetSchemesByProfileAsync(eligibility,HttpContext);

            Scheme? scheme = schemes.FirstOrDefault(scheme => scheme.SchemeID == schemeId);

            return View("PScheme",scheme);
        }
    }
}
