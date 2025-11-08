using Logistics.Api.Models;
using Logistics.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace Logistics.Api.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class InventoryController : ControllerBase
	{
		private readonly IInventoryService _service;
		public InventoryController(IInventoryService service)
		{
			_service = service;
		}

		public record ReceiveRequest(Guid WarehouseId, Guid ProductId, int Quantity);

		[HttpPost("receive")]
		public async Task<ActionResult> Receive([FromBody] ReceiveRequest request, CancellationToken ct)
		{
			await _service.ReceiveAsync(request.WarehouseId, request.ProductId, request.Quantity, ct);
			return NoContent();
		}

		[HttpGet("{warehouseId}")]
		public async Task<ActionResult<List<InventoryRecord>>> GetByWarehouse([FromRoute] Guid warehouseId, CancellationToken ct)
		{
			var list = await _service.GetByWarehouseAsync(warehouseId, ct);
			return Ok(list);
		}
	}
}
