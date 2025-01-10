namespace GovConnect.API
{
    public class PincodeController : Controller
    {
        private readonly PincodeService _pincodeServic;

        public PincodeController(PincodeService pincodeService)
        {
            _pincodeService = pincodeService;
        }

        // Handle the form submission and get data from the API
        [HttpPost]
        public async Task<IActionResult> GetAddressDetails(string pincode)
        {
            if (string.IsNullOrEmpty(pincode))
            {
                ModelState.AddModelError("Pincode", "Pincode cannot be empty.");
                return View("Index");
            }

            var response = await _pincodeService.GetAddressAsync(pincode);
            var addresses = response.SelectMany(x => x.Addresses).ToList();
			return Json(new { success = true, addresses });
		}

    }

}
