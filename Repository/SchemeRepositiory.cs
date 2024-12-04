using System.Text;
namespace GovConnect.Repository
{
    public class SchemeRepository : ISchemeRepository<Scheme>
    {
        private readonly SqlServerDbContext _context;

        public SchemeRepository(SqlServerDbContext context)
        {
            _context = context;
        }

        public async Task<List<Scheme>> GetSchemesByEligibilityAsync(Eligibility eligibility)
        {
            var eligible = await _context.SchemeEligibilities
                .Where(e =>
                    (string.IsNullOrEmpty(eligibility.Gender) || e.Gender == eligibility.Gender) &&
                    (eligibility.Age >= e.MinAge && eligibility.Age <= e.MaxAge) &&
                    (string.IsNullOrEmpty(eligibility.Caste) || e.Caste == eligibility.Caste) &&
                    e.IsDifferentlyAbled == eligibility.IsDifferentlyAbled &&
                    e.IsStudent == eligibility.IsStudent &&
                    e.IsBPL == eligibility.IsBPL
                )
                .ToListAsync();  

            var schemeIds = eligible.Select(e => e.SchemeId).Distinct().ToList();

            var schemes = await _context.GovSchemes
                .Where(s => schemeIds.Contains(s.SchemeID))
                .ToListAsync(); 

            return schemes; 
        }
        public string FormatText(string text, int maxLineLength)
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
        public async Task<Scheme?> GetByIdAsync(int id)
        {
            return await _context.GovSchemes
                .FirstOrDefaultAsync(s => s.SchemeID == id);
        }

        public async Task<List<Scheme>> GetAllAsync()
        {
            return await _context.GovSchemes.ToListAsync();
        }

        public async Task<bool> AddAsync(Scheme entity)
        {
            _context.GovSchemes.Add(entity);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateAsync(Scheme entity)
        {
            _context.GovSchemes.Update(entity);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var scheme = await GetByIdAsync(id);
            if (scheme == null) return false;

            _context.GovSchemes.Remove(scheme);
            await _context.SaveChangesAsync();
            return true;
        }
    }

}
