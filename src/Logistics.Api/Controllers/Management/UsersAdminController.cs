using Logistics.Api.Data;
using Logistics.Api.Models;
using Logistics.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Logistics.Api.Controllers.Management
{
	[ApiController]
	[Route("api/admin/users")]
	[Authorize(Policy = "PlatformAdmin")]
	public class UsersAdminController : ControllerBase
	{
		private readonly ApplicationDbContext _db;
		private readonly IAuthService _auth;
		public UsersAdminController(ApplicationDbContext db, IAuthService auth)
		{
			_db = db;
			_auth = auth;
		}

		public record CreateStationRequest(string Name);
		[HttpPost("stations")]
		public async Task<ActionResult<Station>> CreateStation([FromBody] CreateStationRequest request, CancellationToken ct)
		{
			var station = new Station { Name = request.Name };
			_db.Stations.Add(station);
			await _db.SaveChangesAsync(ct);
			return Ok(station);
		}

		public record CreateUserRequest(string Username, string Password, string RoleName, Guid? StationId);
		[HttpPost]
		public async Task<ActionResult> CreateUser([FromBody] CreateUserRequest request, CancellationToken ct)
		{
			var user = await _auth.RegisterAsync(request.Username, request.Password, request.RoleName, request.StationId, ct);
			return Ok(new { user.Id, user.Username, user.StationId });
		}

		[HttpGet]
		public async Task<ActionResult<List<User>>> ListUsers(CancellationToken ct)
		{
			var list = await _db.Users.AsNoTracking().ToListAsync(ct);
			return Ok(list);
		}
	}
}
