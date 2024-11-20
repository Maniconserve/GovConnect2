using System.Text;
using GovConnect.Data;
using GovConnect.Models;
using Microsoft.EntityFrameworkCore;

namespace GovConnect.Repository
{
    public class SchemeRepository
    {
        private readonly SqlServerDbContext _context;

        public SchemeRepository(SqlServerDbContext context)
        {
            _context = context;
        }

        public async Task<List<Scheme>> GetSchemesByEligibilityAsync(Eligibility eligibility)
        {
            // Ensure Schemes are eager-loaded correctly and the criteria are applied.
            // Step 1: Get the eligibilities based on the given conditions.
            var eligibleEligibilities = await _context.SchemeEligibilities
                .Where(e =>
                    (string.IsNullOrEmpty(eligibility.Gender) || e.Gender == eligibility.Gender) &&
                    (eligibility.Age >= e.MinAge && eligibility.Age <= e.MaxAge) &&
                    (string.IsNullOrEmpty(eligibility.Caste) || e.Caste == eligibility.Caste) &&
                    e.IsDifferentlyAbled == eligibility.IsDifferentlyAbled &&
                    e.IsStudent == eligibility.IsStudent &&
                    e.IsBPL == eligibility.IsBPL
                )
                .ToListAsync();  // This loads all the eligibilities satisfying the condition.

            // Step 2: Get the SchemeIds from the filtered Eligibilities
            var schemeIds = eligibleEligibilities.Select(e => e.SchemeId).Distinct().ToList();

            // Step 3: Load the schemes based on the SchemeIds
            var schemes = await _context.GovSchemes
                .Where(s => schemeIds.Contains(s.SchemeID))
                .ToListAsync();  // This loads the Schemes related to the eligible SchemeIds.

            return schemes;
        }



        public async Task<Scheme> GetSchemeByIdAsync(int schemeId)
        {
            // Query the Schemes table using EF Core and project the result into SchemeViewModel
            var schemeViewModel = await _context.GovSchemes
                    .Where(s => s.SchemeID == schemeId)
                    .Select(s => new Scheme
                    {
                        SchemeID = s.SchemeID,
                        SchemeName = s.SchemeName,
                        Details = s.Details,
                        Eligibility = s.Eligibility,
                        ApplicationProcess = FormatText(s.ApplicationProcess, 160),
                        DocsRequired = FormatText(s.DocsRequired, 160),
                        Attributes = s.Attributes 
                    })
                    .FirstOrDefaultAsync();

            return schemeViewModel;
        }
        public static string FormatText(string text, int maxLineLength)
        {
            var words = text.Split(' ');
            var lines = new List<string>();
            var currentLine = new StringBuilder();

            foreach (var word in words)
            {
                // Check if adding this word would exceed the max line length
                if (currentLine.Length + word.Length + 1 > maxLineLength)
                {
                    lines.Add(currentLine.ToString());
                    currentLine.Clear();
                }

                if (currentLine.Length > 0)
                    currentLine.Append(" ");

                currentLine.Append(word);
            }

            if (currentLine.Length > 0)
                lines.Add(currentLine.ToString());

            // Join lines with newline characters
            return string.Join(Environment.NewLine, lines);
        }

    }

}
