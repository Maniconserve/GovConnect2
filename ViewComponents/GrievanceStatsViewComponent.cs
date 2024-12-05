namespace GovConnect.ViewComponents
{ 
    public class GrievanceStatsViewComponent : ViewComponent
    {
        private readonly SqlServerDbContext _dbContext;  // Replace with your DbContext

        public GrievanceStatsViewComponent(SqlServerDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            // Get the count of grievances by status
            var pendingCount = await _dbContext.DGrievances.CountAsync(g => g.Status == Status.Pending);
            var resolvedCount = await _dbContext.DGrievances.CountAsync(g => g.Status == Status.Completed);
            var inProgressCount = await _dbContext.DGrievances.CountAsync(g => g.Status == Status.InProgress);

            // Create a ViewModel to pass to the view
            var model = new GrievanceStatsViewModel
            {
                PendingCount = pendingCount,
                ResolvedCount = resolvedCount,
                InProgressCount = inProgressCount
            };

            return View(model);
        }
    }

}
