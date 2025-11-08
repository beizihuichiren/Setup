using Logistics.Api.Models;
using Logistics.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace Logistics.Api.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class ProductsController : ControllerBase
	{
		private readonly IProductService _service;
		public ProductsController(IProductService service)
		{
			_service = service;
		}

		public record CreateProductRequest(string Sku, string Name);

		[HttpPost]
		public async Task<ActionResult<Product>> Create([FromBody] CreateProductRequest request, CancellationToken ct)
		{
			var entity = await _service.CreateAsync(request.Sku, request.Name, ct);
			return CreatedAtAction(nameof(GetAll), new { id = entity.Id }, entity);
		}

		[HttpGet]
		public async Task<ActionResult<List<Product>>> GetAll(CancellationToken ct)
		{
			var list = await _service.GetAllAsync(ct);
			return Ok(list);
		}
	}
}
