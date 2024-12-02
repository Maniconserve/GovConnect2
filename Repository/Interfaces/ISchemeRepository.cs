namespace GovConnect.Repository
{
    public interface ISchemeRepository
    {
        // Asynchronous method to get a list of schemes based on eligibility criteria
        Task<List<Scheme>> GetSchemesByEligibilityAsync(Eligibility eligibility);

        // Method to format text based on the max line length
        string FormatText(string text, int maxLineLength);
    }
}
