namespace Logistics.Api.Models
{
	public class AfterSaleCase
	{
		public Guid Id { get; set; } = Guid.NewGuid();
		public Guid OrderId { get; set; }
		public Guid? StationId { get; set; }
		public string Type { get; set; } = "Refund"; // Refund/Return/Issue
		public string Status { get; set; } = "Open"; // Open/Processing/Closed
		public string? Description { get; set; }
		public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
		public DateTime? ClosedAtUtc { get; set; }
	}
}
