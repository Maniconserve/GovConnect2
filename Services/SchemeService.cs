using GovConnect.Models;
using GovConnect.Repository;
namespace GovConnect.Services
{
    public class SchemeService : ISchemeService
    {
        private readonly SchemeRepository _schemeRepository;

        public SchemeService(SchemeRepository schemeRepository)
        {
            _schemeRepository = schemeRepository;
        }

        public async Task<List<Scheme>> GetSchemesByEligibilityAsync(Eligibility eligibility)
        {
            return await _schemeRepository.GetSchemesByEligibilityAsync(eligibility);
        }
    }
}
