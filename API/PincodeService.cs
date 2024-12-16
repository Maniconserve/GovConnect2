using Newtonsoft.Json;
namespace GovConnect.API
{
    public class PincodeService
    {
        private readonly HttpClient _httpClient;

        public PincodeService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<Response>> GetAddressAsync(string pincode)
        {
            var url = $"https://api.postalpincode.in/pincode/{pincode}";

            try
            {
                var response = await _httpClient.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    Console.WriteLine(content);
                    List<Response> addresses = JsonConvert.DeserializeObject<List<Response>>(content);
                    return addresses;
                }
                else
                {
                    // Handle HTTP errors
                    throw new Exception("Error fetching data from API");
                }
            }
            catch (Exception ex)
            {
                // Log the error or handle it as necessary
                return null;
            }
        }
    }

}
