namespace ProjectGreenLens.Repositories.Interfaces
{
    public interface IBaseRepository<T>
    {
        Task<IEnumerable<T>> getAllAsync();
        Task<T> createAsync(T entity);
        Task updateAsync(T entity);
        Task<T?> getByIdAsync(Guid id);
        Task deleteAsync(T entity);
    }
}
