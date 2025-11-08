namespace Logistics.Api.Models
{
	public class Role
	{
		public Guid Id { get; set; } = Guid.NewGuid();
		public string Name { get; set; } = string.Empty; // PlatformAdmin, StationAdmin, Employee
	}

	public class Station
	{
		public Guid Id { get; set; } = Guid.NewGuid();
		public string Name { get; set; } = string.Empty;
	}

	public class User
	{
		public Guid Id { get; set; } = Guid.NewGuid();
		public string Username { get; set; } = string.Empty;
		public string PasswordHash { get; set; } = string.Empty;
		public Guid? StationId { get; set; } // StationAdmin/Employee 所属站点
	}

	public class UserRole
	{
		public Guid Id { get; set; } = Guid.NewGuid();
		public Guid UserId { get; set; }
		public Guid RoleId { get; set; }
	}

	public class EmployeeProfile
	{
		public Guid Id { get; set; } = Guid.NewGuid();
		public Guid UserId { get; set; }
		public string FullName { get; set; } = string.Empty;
	}

	public class OrderOperationRecord
	{
		public Guid Id { get; set; } = Guid.NewGuid();
		public Guid OrderId { get; set; }
		public Guid OperatorUserId { get; set; }
		public DateTime OperatedAtUtc { get; set; } = DateTime.UtcNow;
		public string Action { get; set; } = string.Empty; // e.g., Create, Allocate, Ship, Update
		public string? Note { get; set; }
	}

	public class Earning
	{
		public Guid Id { get; set; } = Guid.NewGuid();
		public Guid UserId { get; set; }
		public decimal Amount { get; set; }
		public DateTime OccurredAtUtc { get; set; } = DateTime.UtcNow;
		public string? Source { get; set; }
	}
}
