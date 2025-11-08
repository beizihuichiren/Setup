using Logistics.Api.Data;
using Logistics.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Logistics.Api.Services
{
	public interface IWarehouseService
	{
		Task<Warehouse> CreateAsync(string name, string address, CancellationToken ct = default);
		Task<List<Warehouse>> GetAllAsync(CancellationToken ct = default);
	}

	public class WarehouseService : IWarehouseService
	{
		private readonly ApplicationDbContext _db;
		public WarehouseService(ApplicationDbContext db)
		{
			_db = db;
		}

		public async Task<Warehouse> CreateAsync(string name, string address, CancellationToken ct = default)
		{
			var entity = new Warehouse { Name = name, Address = address };
			_db.Warehouses.Add(entity);
			await _db.SaveChangesAsync(ct);
			return entity;
		}

		public Task<List<Warehouse>> GetAllAsync(CancellationToken ct = default)
		{
			return _db.Warehouses.AsNoTracking().ToListAsync(ct);
		}
	}
}
