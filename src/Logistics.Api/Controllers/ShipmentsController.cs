using Logistics.Api.Models;
using Logistics.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace Logistics.Api.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class ShipmentsController : ControllerBase
	{
		private readonly IShipmentService _service;
		public ShipmentsController(IShipmentService service)
		{
			_service = service;
		}

		public record ShipOrderRequest(string CourierCode);

		[HttpPost("ship/{orderId}")]
		public async Task<ActionResult<Shipment>> Ship([FromRoute] Guid orderId, [FromBody] ShipOrderRequest request, CancellationToken ct)
		{
			var shipment = await _service.ShipOrderAsync(orderId, request.CourierCode, ct);
			return CreatedAtAction(nameof(Get), new { id = shipment.Id }, shipment);
		}

		[HttpGet("{id}")]
		public async Task<ActionResult<Shipment>> Get([FromRoute] Guid id, CancellationToken ct)
		{
			var shipment = await _service.GetAsync(id, ct);
			if (shipment == null) return NotFound();
			return Ok(shipment);
		}
	}
}
