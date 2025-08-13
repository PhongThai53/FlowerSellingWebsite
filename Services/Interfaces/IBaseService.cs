namespace ProjectGreenLens.Services.Interfaces
{
    public interface IBaseService<T>
    {
        Task<T> getByIdAsync(Guid id);
        Task<IEnumerable<T>> getAllAsync();
        Task<T> createAsync(T entity);
        Task<T> updateAsync(T entity);
        Task<bool> deleteAsync(Guid id);
    }
}
