namespace Logistics.Api.Models
{
	public class Order
	{
		public Guid Id { get; set; } = Guid.NewGuid();
		public Guid CustomerId { get; set; }
		public Guid? StationId { get; set; } // 归属站点（用于二级系统范围）
		public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
		public string Status { get; set; } = "Created"; // Created, Allocated, Shipped
		public List<OrderItem> Items { get; set; } = new();
	}

	public class OrderItem
	{
		public Guid Id { get; set; } = Guid.NewGuid();
		public Guid OrderId { get; set; }
		public Guid ProductId { get; set; }
		public int Quantity { get; set; }
		public Order? Order { get; set; }
	}
}
