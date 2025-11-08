using Logistics.Api.Data;
using Logistics.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Logistics.Api.Services
{
	public interface IProductService
	{
		Task<Product> CreateAsync(string sku, string name, CancellationToken ct = default);
		Task<List<Product>> GetAllAsync(CancellationToken ct = default);
	}

	public class ProductService : IProductService
	{
		private readonly ApplicationDbContext _db;
		public ProductService(ApplicationDbContext db)
		{
			_db = db;
		}

		public async Task<Product> CreateAsync(string sku, string name, CancellationToken ct = default)
		{
			var entity = new Product { Sku = sku, Name = name };
			_db.Products.Add(entity);
			await _db.SaveChangesAsync(ct);
			return entity;
		}

		public Task<List<Product>> GetAllAsync(CancellationToken ct = default)
		{
			return _db.Products.AsNoTracking().ToListAsync(ct);
		}
	}
}
