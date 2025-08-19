using FlowerSellingWebsite.Models.DTOs;
using FlowerSellingWebsite.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics.Contracts;

namespace FlowerSellingWebsite.Controllers
{
    [Route("api/user")]
    [ApiController]
    //[ApiExplorerSettings(GroupName = "Admin")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpPost("list")]
        public async Task<IActionResult> ListUserByStore(UrlQueryParams urlQueryParams)
        {
            var listUser = await _userService.GetUsersAsync(urlQueryParams);

            return Ok(listUser);
        }

        [HttpGet("{publicId}")]
        public async Task<IActionResult> GetById(Guid publicId)
        {
            var user = await _userService.GetUserByIdAsync(publicId);

            return user != null ? Ok(user) : NotFound();
        }

        [HttpPost]
        public async Task<IActionResult> CreateUser(CreateUserRequestDTO request)
        {
            var createUser = await _userService.CreateUserAsync(request);

            return createUser != null ? Ok(createUser) : NotFound();
        }

        [HttpPatch("{publicId}")]
        public async Task<IActionResult> UpdateUser(Guid publicId, UpdateUserRequestDTO request)
        {
            var updatedUser = await _userService.UpdateUserAsync(publicId, request);

            return updatedUser != null ? Ok(updatedUser) : NotFound();
        }

        [HttpDelete("{publicId}")]
        public async Task<IActionResult> DeleteUser(Guid publicId)
        {
            var delUser = await _userService.DeleteUserAsync(publicId);

            return delUser == true ? Ok(delUser) : NotFound();
        }

        [HttpGet("roles")]
        public async Task<IActionResult> GetRoles()
        {
            var roles = await _userService.GetRolesAsync();
            return Ok(roles);
        }
    }
}
