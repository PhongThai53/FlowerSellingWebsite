namespace FlowerSellingWebsite.Controllers
{
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
            public async Task<ActionResult<IEnumerable<T>>> GetAll()
            {
                var items = await _service.getAllAsync();
                return Ok(items);
            }

            [HttpGet("{id}")]
            public async Task<ActionResult<T>> GetById(int id)
            {
                try
                {
                    var entity = await _service.getByIdAsync(id);
                    return Ok(entity);
                }
                catch (KeyNotFoundException ex)
                {
                    return NotFound(new { message = ex.Message });
                }
            }

            [HttpPost]
            public async Task<ActionResult<T>> Create([FromBody] T entity)
            {
                if (entity == null)
                    return BadRequest("Entity cannot be null.");

                var created = await _service.createAsync(entity);
                return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
            }

            [HttpPut("{id}")]
            public async Task<ActionResult<T>> Update(int id, [FromBody] T entity)
            {
                if (id != entity.Id)
                    return BadRequest("Id mismatch.");

                try
                {
                    var updated = await _service.updateAsync(entity);
                    return Ok(updated);
                }
                catch (KeyNotFoundException ex)
                {
                    return NotFound(new { message = ex.Message });
                }
            }

            [HttpDelete("{id}")]
            public async Task<ActionResult> Delete(int id)
            {
                try
                {
                    await _service.deleteAsync(id);
                    return NoContent();
                }
                catch (KeyNotFoundException ex)
                {
                    return NotFound(new { message = ex.Message });
                }
            }
        }
    }
}
