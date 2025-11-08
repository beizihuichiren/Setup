namespace Logistics.Api.Models
{
	public class InventoryRecord
	{
		public Guid Id { get; set; } = Guid.NewGuid();
		public Guid WarehouseId { get; set; }
		public Guid ProductId { get; set; }
		public int Quantity { get; set; }
	}
}
