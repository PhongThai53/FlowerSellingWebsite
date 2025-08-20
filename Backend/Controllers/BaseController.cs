namespace FlowerSellingWebsite.Controllers
{
    using FlowerSellingWebsite.Models.DTOs;
    using global::ProjectGreenLens.Services.Interfaces;
    using Microsoft.AspNetCore.Mvc;

    namespace ProjectGreenLens.Controllers
    {
        [ApiController]
        [Route("api/[controller]")]
        public class BaseController<TCreateDTO, TUpdateDTO, TResponseDTO> : ControllerBase
        {
            private readonly IBaseService<TCreateDTO, TUpdateDTO, TResponseDTO> _service;

            public BaseController(IBaseService<TCreateDTO, TUpdateDTO, TResponseDTO> service)
            {
                _service = service;
            }

            [HttpGet]
            public async Task<IActionResult> GetAll()
            {
                var data = await _service.getAllAsync();
                return Ok(ApiResponse<IEnumerable<TResponseDTO>>.Ok(data));
            }

            [HttpGet("{id:int}")]
            public async Task<IActionResult> GetById(int id)
            {
                try
                {
                    var data = await _service.getByIdAsync(id);
                    return Ok(ApiResponse<TResponseDTO>.Ok(data));
                }
                catch (KeyNotFoundException ex)
                {
                    return NotFound(ApiResponse<TResponseDTO>.Fail(ex.Message));
                }
            }

            [HttpPost]
            public async Task<IActionResult> Create([FromBody] TCreateDTO createDto)
            {
                var data = await _service.createAsync(createDto);
                return Ok(ApiResponse<TResponseDTO>.Ok(data, "Created successfully"));
            }

            [HttpPut("{id:int}")]
            public async Task<IActionResult> Update(int id, [FromBody] TUpdateDTO updateDto)
            {
                try
                {
                    var data = await _service.updateAsync(id, updateDto);
                    return Ok(ApiResponse<TResponseDTO>.Ok(data, "Updated successfully"));
                }
                catch (KeyNotFoundException ex)
                {
                    return NotFound(ApiResponse<TResponseDTO>.Fail(ex.Message));
                }
            }

            [HttpDelete("{id:int}")]
            public async Task<IActionResult> Delete(int id)
            {
                try
                {
                    var success = await _service.deleteAsync(id);
                    return Ok(ApiResponse<bool>.Ok(success, "Deleted successfully"));
                }
                catch (KeyNotFoundException ex)
                {
                    return NotFound(ApiResponse<bool>.Fail(ex.Message));
                }
            }
        }
    }
}
