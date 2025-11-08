using Logistics.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace Logistics.Api.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class AuthController : ControllerBase
	{
		private readonly IAuthService _auth;
		public AuthController(IAuthService auth)
		{
			_auth = auth;
		}

		public record RegisterRequest(string Username, string Password, string RoleName, Guid? StationId);
		public record LoginRequest(string Username, string Password);

		[HttpPost("register")]
		public async Task<ActionResult> Register([FromBody] RegisterRequest request, CancellationToken ct)
		{
			var user = await _auth.RegisterAsync(request.Username, request.Password, request.RoleName, request.StationId, ct);
			return Ok(new { user.Id, user.Username, user.StationId });
		}

		[HttpPost("login")]
		public async Task<ActionResult> Login([FromBody] LoginRequest request, CancellationToken ct)
		{
			var token = await _auth.LoginAsync(request.Username, request.Password, ct);
			return Ok(new { token });
		}
	}
}
