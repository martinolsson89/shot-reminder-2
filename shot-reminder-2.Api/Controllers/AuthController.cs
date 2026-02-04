using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using shot_reminder_2.Application.Use_Cases.Auth.Login;
using shot_reminder_2.Application.Use_Cases.Auth.Register;
using shot_reminder_2.Contracts.Auth;

namespace shot_reminder_2.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly RegisterUserHandler _register;
        private readonly LoginHandler _login;

        public AuthController(RegisterUserHandler register, LoginHandler login)
        {
            _register = register;
            _login = login;
        }

        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterRequest request, CancellationToken ct)
        {
            var userId = await _register.HandleAsync(request.Email, request.Password, request.FirstName, request.LastName, ct);
            return Created(nameof(Register), new { userId });
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<ActionResult<AuthResponse>> Login(LoginRequest request, CancellationToken ct)
        {
            var token = await _login.HandleAsync(request.Email, request.Password, ct);
            return Ok(new AuthResponse(token));
        }
    }
}
