using Newtonsoft.Json;

namespace GovConnect.Services
{
    public class SchemeService : ISchemeService
    {
        private readonly ISchemeRepository _schemeRepository;

        public SchemeService(ISchemeRepository schemeRepository)
        {
            _schemeRepository = schemeRepository;
        }

        public async Task<List<Scheme>> GetSchemesByEligibilityAsync(Eligibility eligibility)
        {
            return await _schemeRepository.GetSchemesByEligibilityAsync(eligibility);
        }
        public async Task<List<Scheme>> GetSchemesByProfileAsync(Eligibility eligibility, HttpContext httpContext)
        {
            var schemesJson = httpContext.Session.GetString("Schemes");

            List<Scheme> schemes;

            if (string.IsNullOrEmpty(schemesJson))
            {
                // If no schemes are in session, get them from the repository
                schemes = await _schemeRepository.GetSchemesByEligibilityAsync(eligibility);

                // Serialize the list to a JSON string and store it in session
                var schemesJsonToStore = JsonConvert.SerializeObject(schemes);
                httpContext.Session.SetString("Schemes", schemesJsonToStore);
            }
            else
            {
                // Deserialize the JSON string back to a list of schemes
                schemes = JsonConvert.DeserializeObject<List<Scheme>>(schemesJson);
            }

            return schemes;
        }
    }
}
