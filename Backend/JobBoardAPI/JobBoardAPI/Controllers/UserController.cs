using JobBoardAPI.Model;
using JobBoardAPI.RestModels;
using JobBoardAPI.Service.Common;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JobBoardAPI.Controllers
{
    [Route("api/user")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _usersService;

        public UserController(IUserService usersService)
        {
            _usersService = usersService ?? throw new ArgumentNullException(nameof(usersService));
        }

        [HttpGet("get-users")]
        public async Task<IActionResult> GetAllUsersAsync()
        {
            var users = await _usersService.GetAllUsersAsync();
            var usersRest = users.Select(u => new UserREST
            {
                Id = u.Id,
                FullName = u.FullName,
                Email = u.Email,
                PasswordHash = u.PasswordHash,
                Role = u.Role
            });
            return Ok(usersRest);
        }

        [HttpGet("get-users/{id}")]
        public async Task<IActionResult> GetUsersAsync(Guid id)
        {
            var user = await _usersService.GetUserByIdAsync(id);
            if (user == null)
                return NotFound();

            var usersRest = new UserREST
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                PasswordHash = user.PasswordHash,
                Role = user.Role
            };
            return Ok(usersRest);
        }

        [HttpPost("create-user")]
        public async Task<IActionResult> CreateUserAsync([FromBody] List<UserREST> usersREST)
        {
            if (usersREST == null || !usersREST.Any())
                return BadRequest("Users cannot be null or empty.");

            var users = usersREST.Select(user => new User
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                PasswordHash = user.PasswordHash,
                Role = user.Role
            }).ToList();

            await _usersService.CreateUserAsync(users);
            return Ok();
        }

        [HttpPut("update-user")]
        public async Task<IActionResult> UpdateUserAsync(Guid id, [FromBody] UserREST userREST)
        {
            if (userREST == null)
                return BadRequest("User cannot be null.");

            var user = new User
            {
                Id = userREST.Id,
                FullName = userREST.FullName,
                Email = userREST.Email,
                PasswordHash = userREST.PasswordHash,
                Role = userREST.Role
            };

            bool updated = await _usersService.UpdateUserAsync(id, user);
            if (!updated)
                return NotFound("User not found or not updated.");

            return Ok("User updated.");
        }

        [HttpDelete("delete-user/{id}")]
        public async Task<IActionResult> DeleteUserAsync(Guid id)
        {
            await _usersService.DeleteUserAsync(id);
            return NoContent();
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Password) ||
                (request.Role != "employer" && request.Role != "candidate"))
                return BadRequest("Invalid registration data.");

            try
            {
                var userId = await _usersService.RegisterAsync(request.Email, request.FullName, request.Password, request.Role);
                return Ok(new { userId });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message ?? "Registration failed." });
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Password))
                return BadRequest("Invalid login data.");

            try
            {
                var user = await _usersService.LoginAsync(request.Email, request.Password);
                if (user == null)
                    return Unauthorized(new { message = "Invalid email or password." });

                return Ok(new { userId = user.Id, role = user.Role });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message ?? "Login failed." });
            }
        }
    }

    public class RegisterRequest
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public string Role { get; set; }
        public string FullName { get; set; }
    }

    public class LoginRequest
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }

    public class UserREST
    {
        public Guid Id { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public string Role { get; set; }
    }
}