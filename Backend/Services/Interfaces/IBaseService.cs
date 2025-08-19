namespace ProjectGreenLens.Services.Interfaces
{
    public interface IBaseService<TCreateDTO, TUpdateDTO, TResponseDTO>
    {
        Task<IEnumerable<TResponseDTO>> getAllAsync();
        Task<TResponseDTO> getByIdAsync(int id);
        Task<TResponseDTO> createAsync(TCreateDTO createDto);
        Task<TResponseDTO> updateAsync(int id, TUpdateDTO updateDto);
        Task<bool> deleteAsync(int id);
    }
}
