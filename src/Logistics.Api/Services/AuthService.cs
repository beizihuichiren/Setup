using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Logistics.Api.Data;
using Logistics.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Logistics.Api.Services
{
	public interface IAuthService
	{
		Task<User> RegisterAsync(string username, string password, string roleName, Guid? stationId, CancellationToken ct = default);
		Task<string> LoginAsync(string username, string password, CancellationToken ct = default);
	}

	public class AuthService : IAuthService
	{
		private readonly ApplicationDbContext _db;
		private readonly IConfiguration _config;

		public AuthService(ApplicationDbContext db, IConfiguration config)
		{
			_db = db;
			_config = config;
		}

		public async Task<User> RegisterAsync(string username, string password, string roleName, Guid? stationId, CancellationToken ct = default)
		{
			if (await _db.Users.AnyAsync(u => u.Username == username, ct))
				throw new InvalidOperationException("用户名已存在");

			var role = await _db.Roles.FirstOrDefaultAsync(r => r.Name == roleName, ct)
				?? throw new InvalidOperationException("角色不存在");

			var user = new User
			{
				Username = username,
				PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
				StationId = stationId
			};
			_db.Users.Add(user);
			_db.UserRoles.Add(new UserRole { UserId = user.Id, RoleId = role.Id });
			await _db.SaveChangesAsync(ct);
			return user;
		}

		public async Task<string> LoginAsync(string username, string password, CancellationToken ct = default)
		{
			var user = await _db.Users.FirstOrDefaultAsync(u => u.Username == username, ct)
				?? throw new InvalidOperationException("用户名或密码错误");
			if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
				throw new InvalidOperationException("用户名或密码错误");

			var roleIds = await _db.UserRoles.Where(ur => ur.UserId == user.Id).Select(ur => ur.RoleId).ToListAsync(ct);
			var roles = await _db.Roles.Where(r => roleIds.Contains(r.Id)).Select(r => r.Name).ToListAsync(ct);

			var claims = new List<Claim>
			{
				new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
				new Claim(ClaimTypes.Name, user.Username),
			};
			foreach (var roleName in roles)
			{
				claims.Add(new Claim(ClaimTypes.Role, roleName));
			}
			if (user.StationId.HasValue)
			{
				claims.Add(new Claim("station_id", user.StationId.Value.ToString()));
			}

			var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
			var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
			var token = new JwtSecurityToken(
				issuer: _config["Jwt:Issuer"],
				audience: _config["Jwt:Audience"],
				claims: claims,
				expires: DateTime.UtcNow.AddHours(8),
				signingCredentials: creds);
			return new JwtSecurityTokenHandler().WriteToken(token);
		}
	}
}
