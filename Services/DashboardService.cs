namespace GovConnect.Services
{
    public class DashboardService
    {
        private readonly SqlServerDbContext _context;

        public DashboardService(SqlServerDbContext context)
        {
            _context = context;
        }

        public OfficerDashboardViewModel GetOfficerDashboard(int officerId)
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
            var pendingGrievances = grievances.Count(g => g.Status == "Pending");
            var resolvedGrievances = grievances.Count(g => g.Status == "Resolved");

            // Dummy image for now
            var officerImage = "/path/to/dummy/image.jpg";

            // Return the dashboard view model
            return new OfficerDashboardViewModel
            {
                OfficerName = officer.OfficerName,
                OfficerDesignation = officer.OfficerDesignation,
                DepartmentId = officer.DeptId,
                TotalGrievances = totalGrievances,
                PendingGrievances = pendingGrievances,
                ResolvedGrievances = resolvedGrievances,
                AssignedGrievances = grievances,
                OfficerImage = officerImage
            };
        }
    }

}
