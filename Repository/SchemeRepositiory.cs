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

        public async Task<Scheme> GetSchemeByIdAsync(int schemeId)
        {
            // Query the Schemes table using EF Core and project the result into SchemeViewModel
            var schemeViewModel = await _context.Schemes
                .Where(s => s.SchemeID == schemeId)
                .Select(s => new Scheme
                {
                    SchemeID = s.SchemeID,
                    Details = s.Details,
                    Eligibility = s.Eligibility,
                    ApplicationProcess = s.ApplicationProcess,
                    DocsRequired = s.DocsRequired
                })
                .FirstOrDefaultAsync();

            return schemeViewModel;
        }
    }

}
