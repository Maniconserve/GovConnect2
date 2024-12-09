namespace GovConnect.Services.Interfaces
{
    public interface IEntityService<T> where T : class
    {
        Task<List<T>> GetAllAsync();
        Task<T?> GetByIdAsync(int id);
        Task<bool> UpdateAsync(T entity);
    }

}
