using Logistics.Api.Data;
using Logistics.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Logistics.Api.Services
{
	public interface IShipmentService
	{
		Task<Shipment> ShipOrderAsync(Guid orderId, string courierCode, CancellationToken ct = default);
		Task<Shipment?> GetAsync(Guid shipmentId, CancellationToken ct = default);
	}

	public class ShipmentService : IShipmentService
	{
		private readonly ApplicationDbContext _db;
		public ShipmentService(ApplicationDbContext db)
		{
			_db = db;
		}

		public async Task<Shipment> ShipOrderAsync(Guid orderId, string courierCode, CancellationToken ct = default)
		{
			var order = await _db.Orders.FirstOrDefaultAsync(o => o.Id == orderId, ct);
			if (order == null) throw new KeyNotFoundException("订单不存在");
			if (order.Status != "Allocated") throw new InvalidOperationException("订单未分配，不能发运");

			var trackingNumber = $"TRK-{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid().ToString()[..8]}";
			var shipment = new Shipment
			{
				OrderId = orderId,
				CourierCode = courierCode,
				TrackingNumber = trackingNumber,
				ShippedAtUtc = DateTime.UtcNow
			};
			_db.Shipments.Add(shipment);

			order.Status = "Shipped";
			await _db.SaveChangesAsync(ct);
			return shipment;
		}

		public Task<Shipment?> GetAsync(Guid shipmentId, CancellationToken ct = default)
		{
			return _db.Shipments.FirstOrDefaultAsync(s => s.Id == shipmentId, ct);
		}
	}
}
