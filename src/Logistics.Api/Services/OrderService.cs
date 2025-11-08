using Logistics.Api.Data;
using Logistics.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Logistics.Api.Services
{
	public interface IOrderService
	{
		Task<Order> CreateAsync(Guid customerId, IEnumerable<(Guid productId, int quantity)> items, CancellationToken ct = default, Guid? stationId = null);
		Task<Order?> GetAsync(Guid orderId, CancellationToken ct = default);
		Task AllocateAsync(Guid orderId, Guid warehouseId, CancellationToken ct = default);
	}

	public class OrderService : IOrderService
	{
		private readonly ApplicationDbContext _db;
		private readonly IInventoryService _inventoryService;
		public OrderService(ApplicationDbContext db, IInventoryService inventoryService)
		{
			_db = db;
			_inventoryService = inventoryService;
		}

		public async Task<Order> CreateAsync(Guid customerId, IEnumerable<(Guid productId, int quantity)> items, CancellationToken ct = default, Guid? stationId = null)
		{
			var order = new Order
			{
				CustomerId = customerId,
				StationId = stationId,
				Status = "Created",
				Items = items.Select(i => new OrderItem { ProductId = i.productId, Quantity = i.quantity }).ToList()
			};
			_db.Orders.Add(order);
			await _db.SaveChangesAsync(ct);
			return order;
		}

		public Task<Order?> GetAsync(Guid orderId, CancellationToken ct = default)
		{
			return _db.Orders.Include(o => o.Items).FirstOrDefaultAsync(o => o.Id == orderId, ct);
		}

		public async Task AllocateAsync(Guid orderId, Guid warehouseId, CancellationToken ct = default)
		{
			var order = await _db.Orders.Include(o => o.Items).FirstOrDefaultAsync(o => o.Id == orderId, ct);
			if (order == null) throw new KeyNotFoundException("订单不存在");
			if (order.Status != "Created") throw new InvalidOperationException("订单状态不可分配");

			await _inventoryService.AllocateForOrderAsync(
				warehouseId,
				order.Items.Select(i => (i.ProductId, i.Quantity)),
				ct);

			order.Status = "Allocated";
			await _db.SaveChangesAsync(ct);
		}
	}
}
