using Logistics.Api.Models;
using Logistics.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace Logistics.Api.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class OrdersController : ControllerBase
	{
		private readonly IOrderService _service;
		public OrdersController(IOrderService service)
		{
			_service = service;
		}

		public record OrderItemInput(Guid ProductId, int Quantity);
		public record CreateOrderRequest(Guid CustomerId, List<OrderItemInput> Items, Guid? StationId);

		[HttpPost]
		public async Task<ActionResult<Order>> Create([FromBody] CreateOrderRequest request, CancellationToken ct)
		{
			var order = await _service.CreateAsync(
				request.CustomerId,
				request.Items.Select(i => (i.ProductId, i.Quantity)),
				ct,
				request.StationId);
			return CreatedAtAction(nameof(Get), new { orderId = order.Id }, order);
		}

		[HttpGet("{orderId}")]
		public async Task<ActionResult<Order>> Get([FromRoute] Guid orderId, CancellationToken ct)
		{
			var order = await _service.GetAsync(orderId, ct);
			if (order == null) return NotFound();
			return Ok(order);
		}

		public record AllocateRequest(Guid WarehouseId);

		[HttpPost("{orderId}/allocate")]
		public async Task<ActionResult> Allocate([FromRoute] Guid orderId, [FromBody] AllocateRequest request, CancellationToken ct)
		{
			await _service.AllocateAsync(orderId, request.WarehouseId, ct);
			return NoContent();
		}
	}
}
