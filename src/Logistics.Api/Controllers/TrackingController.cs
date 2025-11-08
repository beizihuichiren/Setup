using Logistics.Api.Models;
using Logistics.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace Logistics.Api.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class TrackingController : ControllerBase
	{
		private readonly ITrackingService _service;
		public TrackingController(ITrackingService service)
		{
			_service = service;
		}

		public record AddEventRequest(string Status, string? Location, string? Note);

		[HttpPost("{shipmentId}/events")]
		public async Task<ActionResult<TrackingEvent>> AddEvent([FromRoute] Guid shipmentId, [FromBody] AddEventRequest request, CancellationToken ct)
		{
			var ev = await _service.AddEventAsync(shipmentId, request.Status, request.Location, request.Note, ct);
			return CreatedAtAction(nameof(GetByTrackingNumber), new { trackingNumber = "" }, ev);
		}

		[HttpGet("by-number/{trackingNumber}")]
		public async Task<ActionResult<List<TrackingEvent>>> GetByTrackingNumber([FromRoute] string trackingNumber, CancellationToken ct)
		{
			var list = await _service.GetByTrackingNumberAsync(trackingNumber, ct);
			return Ok(list);
		}
	}
}
