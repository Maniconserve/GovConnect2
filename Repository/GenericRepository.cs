namespace GovConnect.Repository
{
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        private readonly SqlServerDbContext _context;
        private readonly DbSet<T> _dbSet;

        public GenericRepository(SqlServerDbContext context)
        {
            _context = context;
            _dbSet = _context.Set<T>();
        }

        public async Task<List<T>> GetAllAsync()
        {
            return await _dbSet.ToListAsync();
        }

        public async Task<T?> GetByIdAsync(int id)
        {
            return await _dbSet.FindAsync(id);
        }

        public async Task<bool> UpdateAsync(T entity)
        {
            _dbSet.Update(entity);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
