namespace FlowerSellingWebsite.Controllers
{
    using FlowerSellingWebsite.Models.DTOs;
    using FlowerSellingWebsite.Models.Entities;
    using global::ProjectGreenLens.Services.Interfaces;
    using Microsoft.AspNetCore.Mvc;

    namespace ProjectGreenLens.Controllers
    {
        [ApiController]
        [Route("api/[controller]")]
        public class BaseController<T> : ControllerBase where T : BaseEntity
        {
            private readonly IBaseService<T> _service;

            public BaseController(IBaseService<T> service)
            {
                _service = service ?? throw new ArgumentNullException(nameof(service));
            }

            [HttpGet]
            public async Task<ActionResult<ApiResponse<IEnumerable<T>>>> GetAll()
            {
                var items = await _service.getAllAsync();
                return Ok(ApiResponse<IEnumerable<T>>.Ok(items));
            }

            [HttpGet("{id}")]
            public async Task<ActionResult<ApiResponse<T>>> GetById(int id)
            {
                try
                {
                    var entity = await _service.getByIdAsync(id);
                    return Ok(ApiResponse<T>.Ok(entity));
                }
                catch (KeyNotFoundException ex)
                {
                    return NotFound(ApiResponse<T>.Fail(ex.Message));
                }
            }

            [HttpPost]
            public async Task<ActionResult<ApiResponse<T>>> Create([FromBody] T entity)
            {
                if (entity == null)
                    return BadRequest(ApiResponse<T>.Fail("Entity cannot be null."));

                var created = await _service.createAsync(entity);
                return CreatedAtAction(nameof(GetById), new { id = created.Id }, ApiResponse<T>.Ok(created, "Created successfully"));
            }

            [HttpPut("{id}")]
            public async Task<ActionResult<ApiResponse<T>>> Update(int id, [FromBody] T entity)
            {
                if (id != entity.Id)
                    return BadRequest(ApiResponse<T>.Fail("Id mismatch."));

                try
                {
                    var updated = await _service.updateAsync(entity);
                    return Ok(ApiResponse<T>.Ok(updated, "Updated successfully"));
                }
                catch (KeyNotFoundException ex)
                {
                    return NotFound(ApiResponse<T>.Fail(ex.Message));
                }
            }

            [HttpDelete("{id}")]
            public async Task<ActionResult<ApiResponse<string>>> Delete(int id)
            {
                try
                {
                    await _service.deleteAsync(id);
                    return Ok(ApiResponse<string>.Ok("Deleted successfully"));
                }
                catch (KeyNotFoundException ex)
                {
                    return NotFound(ApiResponse<string>.Fail(ex.Message));
                }
            }
        }
    }
}
