namespace GovConnect.API
{
    public class PincodeController : Controller
    {
        private readonly PostOfficeService _postOfficeService;

        public PincodeController(PostOfficeService postOfficeService)
        {
            _postOfficeService = postOfficeService;
        }

        // Display the initial form to input the PIN code
        public IActionResult Index()
        {
            return View();
        }

        // Handle the form submission and get data from the API
        [HttpPost]
        public async Task<IActionResult> GetPostOfficeDetails(string pincode)
        {
            if (string.IsNullOrEmpty(pincode))
            {
                ModelState.AddModelError("Pincode", "Pincode cannot be empty.");
                return View("Index");
            }

            var postOfficeResponse = await _postOfficeService.GetPostOfficeDataAsync(pincode);
            if (postOfficeResponse == null || postOfficeResponse.Status == "Error" || postOfficeResponse.PostOffice == null)
            {
                ViewBag.Message = "No records found for the given PIN code.";
                return View("Index");
            }

            return View("PincodeDetails", postOfficeResponse);
        }

    }

}
