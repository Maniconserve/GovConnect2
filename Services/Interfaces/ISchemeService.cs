
namespace GovConnect.Services
{
    public interface ISchemeService
    {
        Task<List<Scheme>> GetSchemesByEligibilityAsync(Eligibility eligibility);
        Task<List<Scheme>> GetSchemesByProfileAsync(Eligibility eligibility,HttpContext httpContext);
    }
}
