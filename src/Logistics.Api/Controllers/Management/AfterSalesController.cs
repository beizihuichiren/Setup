using Logistics.Api.Data;
using Logistics.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Logistics.Api.Controllers.Management
{
	[ApiController]
	[Route("api/admin/after-sales")]
	[Authorize]
	public class AfterSalesController : ControllerBase
	{
		private readonly ApplicationDbContext _db;
		public AfterSalesController(ApplicationDbContext db)
		{
			_db = db;
		}

		public record CreateCaseRequest(Guid OrderId, string Type, string? Description);
		[HttpPost]
		public async Task<ActionResult<AfterSaleCase>> Create([FromBody] CreateCaseRequest request, CancellationToken ct)
		{
			Guid? stationId = User.IsInRole("PlatformAdmin") ? null : User.FindFirst("station_id")?.Value is string s ? Guid.Parse(s) : null;
			var order = await _db.Orders.FirstOrDefaultAsync(o => o.Id == request.OrderId, ct);
			if (order == null) return NotFound("订单不存在");
			if (stationId.HasValue && order.StationId != stationId) return Forbid();

			var c = new AfterSaleCase { OrderId = request.OrderId, Type = request.Type, Description = request.Description, StationId = order.StationId };
			_db.AfterSaleCases.Add(c);
			await _db.SaveChangesAsync(ct);
			return Ok(c);
		}

		[HttpGet]
		public async Task<ActionResult<List<AfterSaleCase>>> List(CancellationToken ct)
		{
			Guid? stationId = User.IsInRole("PlatformAdmin") ? null : User.FindFirst("station_id")?.Value is string s ? Guid.Parse(s) : null;
			var query = _db.AfterSaleCases.AsQueryable();
			if (stationId.HasValue) query = query.Where(x => x.StationId == stationId);
			var list = await query.OrderByDescending(x => x.CreatedAtUtc).Take(200).ToListAsync(ct);
			return Ok(list);
		}

		public record UpdateStatusRequest(string Status, string? Note);
		[HttpPost("{id}/status")]
		public async Task<ActionResult> UpdateStatus([FromRoute] Guid id, [FromBody] UpdateStatusRequest request, CancellationToken ct)
		{
			Guid? stationId = User.IsInRole("PlatformAdmin") ? null : User.FindFirst("station_id")?.Value is string s ? Guid.Parse(s) : null;
			var c = await _db.AfterSaleCases.FirstOrDefaultAsync(x => x.Id == id, ct);
			if (c == null) return NotFound();
			if (stationId.HasValue && c.StationId != stationId) return Forbid();
			c.Status = request.Status;
			if (request.Status == "Closed") c.ClosedAtUtc = DateTime.UtcNow;
			await _db.SaveChangesAsync(ct);
			return NoContent();
		}
	}
}
