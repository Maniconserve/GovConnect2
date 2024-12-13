using Newtonsoft.Json;
namespace GovConnect.API
{

    public class PostOfficeService
    {
        private readonly HttpClient _httpClient;

        public PostOfficeService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<PostOfficeResponse> GetPostOfficeDataAsync(string pincode)
        {
            var url = $"https://api.postalpincode.in/pincode/{pincode}";

            try
            {
                var response = await _httpClient.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    Console.WriteLine(content);
                    var postOffices = JsonConvert.DeserializeObject<PostOfficeResponse>(content);
                    return postOffices;

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
