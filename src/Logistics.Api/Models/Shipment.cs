namespace Logistics.Api.Models
{
	public class Shipment
	{
		public Guid Id { get; set; } = Guid.NewGuid();
		public Guid OrderId { get; set; }
		public string TrackingNumber { get; set; } = string.Empty;
		public string CourierCode { get; set; } = string.Empty; // e.g., SF, JD, YT
		public DateTime ShippedAtUtc { get; set; } = DateTime.UtcNow;
	}

	public class TrackingEvent
	{
		public Guid Id { get; set; } = Guid.NewGuid();
		public Guid ShipmentId { get; set; }
		public DateTime OccurredAtUtc { get; set; } = DateTime.UtcNow;
		public string Status { get; set; } = string.Empty; // e.g., PickedUp, InTransit, Delivered
		public string? Location { get; set; }
		public string? Note { get; set; }
	}
}
