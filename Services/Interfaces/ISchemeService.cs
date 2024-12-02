
namespace GovConnect.Services
{
    public interface ISchemeService
    {
        Task<List<Scheme>> GetSchemesByEligibilityAsync(Eligibility eligibility);
    }
}
