using Logistics.Api.Models;
using Logistics.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Logistics.Api.Controllers.Management
{
	[ApiController]
	[Route("api/admin/products")]
	[Authorize(Policy = "PlatformAdmin")] // 平台管理员；站点管理员如需复用，请复制控制器或放宽策略
	public class ProductsAdminController : ControllerBase
	{
		private readonly IProductService _products;
		public ProductsAdminController(IProductService products)
		{
			_products = products;
		}

		public record UpsertProductRequest(string Sku, string Name);

		[HttpPost]
		public async Task<ActionResult<Product>> Create([FromBody] UpsertProductRequest request, CancellationToken ct)
		{
			var p = await _products.CreateAsync(request.Sku, request.Name, ct);
			return Ok(p);
		}

		[HttpGet]
		public async Task<ActionResult<List<Product>>> List(CancellationToken ct)
		{
			var items = await _products.GetAllAsync(ct);
			return Ok(items);
		}
	}
}
