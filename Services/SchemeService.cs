namespace GovConnect.Services
{
    public class SchemeService : ISchemeService
    {
        private readonly ISchemeRepository<Scheme> _schemeRepository;

        public SchemeService(ISchemeRepository<Scheme> schemeRepository)
        {
            _schemeRepository = schemeRepository;
        }

        public async Task<List<Scheme>> GetSchemesByEligibilityAsync(Eligibility eligibility)
        {
            return await _schemeRepository.GetSchemesByEligibilityAsync(eligibility);
        }
    }
}
