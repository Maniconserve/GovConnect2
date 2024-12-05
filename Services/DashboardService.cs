namespace GovConnect.Services
{
    public class DashboardService
    {
        private SqlServerDbContext _context;
        private UserManager<Citizen> _userManager;

        public DashboardService(SqlServerDbContext context, UserManager<Citizen> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public OfficerDashboardViewModel GetOfficerDashboard(String officerId)
        {
            // Retrieve officer details
            var officer = _context.PoliceOfficers.FirstOrDefault(o => o.OfficerId == officerId);
            if (officer == null) return null;

            // Retrieve grievances assigned to the officer
            var grievances = _context.DGrievances
                .Where(g => g.OfficerId == officerId)
                .Select(g => new Grievance
                {
                    GrievanceID = g.GrievanceID,
                    Title = g.Title,
                    Status = g.Status,
                    CreatedAt = g.CreatedAt
                })
                .ToList();

            // Summary statistics
            var totalGrievances = grievances.Count;
            var pendingGrievances = grievances.Count(g => g.Status == Status.Pending);
            var resolvedGrievances = grievances.Count(g => g.Status == Status.Completed);

            // Dummy image for now
            var officerImage = "/path/to/dummy/image.jpg";
            // Return the dashboard view model
            return new OfficerDashboardViewModel
            {
                OfficerDesignation = officer.OfficerDesignation,
                DepartmentId = officer.DepartmentId,
                TotalGrievances = totalGrievances,
                PendingGrievances = pendingGrievances,
                ResolvedGrievances = resolvedGrievances,
                AssignedGrievances = grievances,
                OfficerImage = officerImage
            };
        }
    }

}
