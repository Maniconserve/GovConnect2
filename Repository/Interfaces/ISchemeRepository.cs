using GovConnect.Repository.Interfaces;

namespace GovConnect.Repository
{
    public interface ISchemeRepository<T> : IGenericRepository<T> where T : class
    {
        Task<List<Scheme>> GetSchemesByEligibilityAsync(Eligibility eligibility);
        string FormatText(string text, int maxLineLength);
    }
}
