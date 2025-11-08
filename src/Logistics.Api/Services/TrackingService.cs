using Logistics.Api.Data;
using Logistics.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Logistics.Api.Services
{
	public interface ITrackingService
	{
		Task<TrackingEvent> AddEventAsync(Guid shipmentId, string status, string? location, string? note, CancellationToken ct = default);
		Task<List<TrackingEvent>> GetByTrackingNumberAsync(string trackingNumber, CancellationToken ct = default);
	}

	public class TrackingService : ITrackingService
	{
		private readonly ApplicationDbContext _db;
		public TrackingService(ApplicationDbContext db)
		{
			_db = db;
		}

		public async Task<TrackingEvent> AddEventAsync(Guid shipmentId, string status, string? location, string? note, CancellationToken ct = default)
		{
			var exists = await _db.Shipments.AnyAsync(s => s.Id == shipmentId, ct);
			if (!exists) throw new KeyNotFoundException("发运不存在");

			var ev = new TrackingEvent
			{
				ShipmentId = shipmentId,
				Status = status,
				Location = location,
				Note = note,
				OccurredAtUtc = DateTime.UtcNow
			};
			_db.TrackingEvents.Add(ev);
			await _db.SaveChangesAsync(ct);
			return ev;
		}

		public async Task<List<TrackingEvent>> GetByTrackingNumberAsync(string trackingNumber, CancellationToken ct = default)
		{
			var shipment = await _db.Shipments.FirstOrDefaultAsync(s => s.TrackingNumber == trackingNumber, ct);
			if (shipment == null) return new List<TrackingEvent>();
			return await _db.TrackingEvents.Where(e => e.ShipmentId == shipment.Id)
				.OrderBy(e => e.OccurredAtUtc)
				.AsNoTracking()
				.ToListAsync(ct);
		}
	}
}
