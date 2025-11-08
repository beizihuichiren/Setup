namespace Logistics.Api.Models
{
	public class Courier
	{
		public Guid Id { get; set; } = Guid.NewGuid();
		public string Code { get; set; } = string.Empty; // internal code
		public string Name { get; set; } = string.Empty;
	}
}
