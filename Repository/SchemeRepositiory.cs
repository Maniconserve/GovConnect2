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
            var eligibleEligibilities = await _context.SchemeEligibilities
                .Where(e =>
                    (string.IsNullOrEmpty(eligibility.Gender) || e.Gender == eligibility.Gender) &&
                    (eligibility.Age >= e.MinAge && eligibility.Age <= e.MaxAge) &&
                    (string.IsNullOrEmpty(eligibility.Caste) || e.Caste == eligibility.Caste) &&
                    e.IsDifferentlyAbled == eligibility.IsDifferentlyAbled &&
                    e.IsStudent == eligibility.IsStudent &&
                    e.IsBPL == eligibility.IsBPL
                )
                .ToListAsync();  

            var schemeIds = eligibleEligibilities.Select(e => e.SchemeId).Distinct().ToList();

            var schemes = await _context.GovSchemes
                .Where(s => schemeIds.Contains(s.SchemeID))
                .ToListAsync(); 

            return schemes;
        }
        public static string FormatText(string text, int maxLineLength)
        {
            var words = text.Split(' ');
            var lines = new List<string>();
            var currentLine = new StringBuilder();

            foreach (var word in words)
            {
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

            return string.Join(Environment.NewLine, lines);
        }

    }

}
