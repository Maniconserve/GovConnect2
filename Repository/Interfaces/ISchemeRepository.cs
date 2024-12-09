namespace GovConnect.Repository
{
    public interface ISchemeRepository
    {
        Task<List<Scheme>> GetSchemesByEligibilityAsync(Eligibility eligibility);
        string FormatText(string text, int maxLineLength);
    }
}
