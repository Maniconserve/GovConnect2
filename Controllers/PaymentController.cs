using System.Text;
using Newtonsoft.Json;

namespace GovConnect.Controllers
{
    public class PaymentController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;

        public PaymentController(IConfiguration configuration, HttpClient httpClient)
        {
            _configuration = configuration;
            _httpClient = httpClient;
        }

        // Show Payment page with Razorpay Key
        public IActionResult Index()
        {
            var razorpayKeyId = _configuration["Razorpay:KeyId"];
            ViewData["RazorpayKeyId"] = razorpayKeyId;

            return View();
        }

        // Create Order in Razorpay
        [HttpPost]
        public async Task<IActionResult> CreateOrder(decimal amount)
        {
            try
            {
                var razorpayKeyId = _configuration["Razorpay:KeyId"];
                var razorpaySecretKey = _configuration["Razorpay:SecretKey"];

                var orderDetails = new
                {
                    amount = amount * 100, // Amount in paise (1 INR = 100 paise)
                    currency = "INR",
                    payment_capture = 1 // Automatic capture of payment
                };

                var jsonContent = new StringContent(JsonConvert.SerializeObject(orderDetails), Encoding.UTF8, "application/json");

                var request = new HttpRequestMessage
                {
                    Method = HttpMethod.Post,
                    RequestUri = new Uri("https://api.razorpay.com/v1/orders"),
                    Headers =
                    {
                        { "Authorization", $"Basic {Convert.ToBase64String(Encoding.ASCII.GetBytes($"{razorpayKeyId}:{razorpaySecretKey}"))}" }
                    },
                    Content = jsonContent
                };

                // Send request to Razorpay API to create an order
                var response = await _httpClient.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    var responseBody = await response.Content.ReadAsStringAsync();
                    var order = JsonConvert.DeserializeObject<dynamic>(responseBody);
                    return Json(order);  // Return Razorpay Order ID
                }

                return Json(new { error = "Failed to create order" });
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }

        // Verify the payment after payment is successful
        [HttpPost]
        public async Task<IActionResult> VerifyPayment(string razorpayPaymentId, string razorpayOrderId, string razorpaySignature)
        {
            try
            {
                var razorpayKeyId = _configuration["Razorpay:KeyId"];
                var razorpaySecretKey = _configuration["Razorpay:SecretKey"];

                // Verify signature
                var generatedSignature = GenerateSignature(razorpayOrderId, razorpayPaymentId, razorpaySecretKey);

                if (generatedSignature == razorpaySignature)
                {
                    // Payment is successful, you can store payment info in database
                    return View("PaymentSuccess");
                }
                else
                {
                    return View("PaymentFailure");
                }
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }

        private string GenerateSignature(string orderId, string paymentId, string secretKey)
        {
            var body = $"{orderId}|{paymentId}";
            using (var hmac = new System.Security.Cryptography.HMACSHA256(Encoding.ASCII.GetBytes(secretKey)))
            {
                var hashBytes = hmac.ComputeHash(Encoding.ASCII.GetBytes(body));
                return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
            }
        }
    }
}
