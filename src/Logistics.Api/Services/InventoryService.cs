using Logistics.Api.Data;
using Logistics.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Logistics.Api.Services
{
	public interface IInventoryService
	{
		Task ReceiveAsync(Guid warehouseId, Guid productId, int quantity, CancellationToken ct = default);
		Task<List<InventoryRecord>> GetByWarehouseAsync(Guid warehouseId, CancellationToken ct = default);
		Task AllocateForOrderAsync(Guid warehouseId, IEnumerable<(Guid productId, int quantity)> items, CancellationToken ct = default);
	}

	public class InventoryService : IInventoryService
	{
		private readonly ApplicationDbContext _db;
		public InventoryService(ApplicationDbContext db)
		{
			_db = db;
		}

		public async Task ReceiveAsync(Guid warehouseId, Guid productId, int quantity, CancellationToken ct = default)
		{
			var rec = await _db.InventoryRecords.FirstOrDefaultAsync(x => x.WarehouseId == warehouseId && x.ProductId == productId, ct);
			if (rec == null)
			{
				rec = new InventoryRecord { WarehouseId = warehouseId, ProductId = productId, Quantity = 0 };
				_db.InventoryRecords.Add(rec);
			}
			rec.Quantity += quantity;
			await _db.SaveChangesAsync(ct);
		}

		public Task<List<InventoryRecord>> GetByWarehouseAsync(Guid warehouseId, CancellationToken ct = default)
		{
			return _db.InventoryRecords.Where(x => x.WarehouseId == warehouseId).AsNoTracking().ToListAsync(ct);
		}

		public async Task AllocateForOrderAsync(Guid warehouseId, IEnumerable<(Guid productId, int quantity)> items, CancellationToken ct = default)
		{
			foreach (var (productId, quantity) in items)
			{
				var rec = await _db.InventoryRecords.FirstOrDefaultAsync(x => x.WarehouseId == warehouseId && x.ProductId == productId, ct);
				if (rec == null || rec.Quantity < quantity)
				{
					throw new InvalidOperationException($"库存不足: product {productId}");
				}
				rec.Quantity -= quantity;
			}
			await _db.SaveChangesAsync(ct);
		}
	}
}
