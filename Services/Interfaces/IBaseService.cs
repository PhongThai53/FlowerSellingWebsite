namespace ProjectGreenLens.Services.Interfaces
{
    public interface IBaseService<T>
    {
        Task<T> getByIdAsync(int id);
        Task<IEnumerable<T>> getAllAsync();
        Task<T> createAsync(T entity);
        Task<T> updateAsync(T entity);
        Task<bool> deleteAsync(int id);
    }
}
