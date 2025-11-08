using Logistics.Api.Models;
using Logistics.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Logistics.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace Logistics.Api.Controllers.Management
{
	[ApiController]
	[Route("api/admin/orders")]
	public class OrdersAdminController : ControllerBase
	{
		private readonly ApplicationDbContext _db;
		public OrdersAdminController(ApplicationDbContext db)
		{
			_db = db;
		}

		// 平台管理员：可见全部；站点管理员/员工：限制到 station_id
		[HttpGet("{orderId}")]
		[Authorize]
		public async Task<ActionResult<Order>> Get([FromRoute] Guid orderId, CancellationToken ct)
		{
			var order = await _db.Orders.Include(o => o.Items).FirstOrDefaultAsync(o => o.Id == orderId, ct);
			if (order == null) return NotFound();

			if (User.IsInRole("PlatformAdmin")) return Ok(order);

			var stationId = User.FindFirst("station_id")?.Value;
			if (stationId == null) return Forbid();
			if (order.StationId?.ToString() != stationId) return Forbid();
			return Ok(order);
		}
	}
}
