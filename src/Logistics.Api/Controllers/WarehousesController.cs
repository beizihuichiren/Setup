using Logistics.Api.Models;
using Logistics.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace Logistics.Api.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class WarehousesController : ControllerBase
	{
		private readonly IWarehouseService _service;
		public WarehousesController(IWarehouseService service)
		{
			_service = service;
		}

		public record CreateWarehouseRequest(string Name, string Address);

		[HttpPost]
		public async Task<ActionResult<Warehouse>> Create([FromBody] CreateWarehouseRequest request, CancellationToken ct)
		{
			var entity = await _service.CreateAsync(request.Name, request.Address, ct);
			return CreatedAtAction(nameof(GetAll), new { id = entity.Id }, entity);
		}

		[HttpGet]
		public async Task<ActionResult<List<Warehouse>>> GetAll(CancellationToken ct)
		{
			var list = await _service.GetAllAsync(ct);
			return Ok(list);
		}
	}
}
