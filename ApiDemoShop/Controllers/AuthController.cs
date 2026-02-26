using ApiDemoShop.Services;
using LibDemoShop;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ApiDemoShop.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("signup")]
        public async Task<ActionResult<AuthResponseDTO>> SignUp([FromBody] CreateUserDTO request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new AuthResponseDTO
                {
                    Success = false,
                    Message = "Некорректные данные"
                });
            }

            var result = await _authService.SignUpAsync(request);

            if (result.Success)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }

        [HttpPost("signin")]
        public async Task<ActionResult<AuthResponseDTO>> SignIn([FromBody] LoginDTO request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new AuthResponseDTO
                {
                    Success = false,
                    Message = "Некорректные данные"
                });
            }

            var result = await _authService.SignInAsync(request);

            if (result.Success)
            {
                return Ok(result);
            }

            return Unauthorized(result);
        }

        [HttpGet("user/{id}")]
        public async Task<ActionResult<UserInfoDTO>> GetUser(int id)
        {
            var user = await _authService.GetUserByIdAsync(id);

            if (user == null)
            {
                return NotFound(new { message = "Пользователь не найден" });
            }

            return Ok(user);
        }

    }
}
