using Microsoft.EntityFrameworkCore;
using Logistics.Api.Models;

namespace Logistics.Api.Data
{
	public class ApplicationDbContext : DbContext
	{
		public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
		{
		}

		public DbSet<Warehouse> Warehouses => Set<Warehouse>();
		public DbSet<Product> Products => Set<Product>();
		public DbSet<InventoryRecord> InventoryRecords => Set<InventoryRecord>();
		public DbSet<Customer> Customers => Set<Customer>();
		public DbSet<Order> Orders => Set<Order>();
		public DbSet<OrderItem> OrderItems => Set<OrderItem>();
		public DbSet<Shipment> Shipments => Set<Shipment>();
		public DbSet<TrackingEvent> TrackingEvents => Set<TrackingEvent>();
		public DbSet<Courier> Couriers => Set<Courier>();

		public DbSet<User> Users => Set<User>();
		public DbSet<Role> Roles => Set<Role>();
		public DbSet<UserRole> UserRoles => Set<UserRole>();
		public DbSet<Station> Stations => Set<Station>();
		public DbSet<EmployeeProfile> EmployeeProfiles => Set<EmployeeProfile>();
		public DbSet<OrderOperationRecord> OrderOperationRecords => Set<OrderOperationRecord>();
		public DbSet<Earning> Earnings => Set<Earning>();
		public DbSet<AfterSaleCase> AfterSaleCases => Set<AfterSaleCase>();

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			modelBuilder.Entity<OrderItem>()
				.HasOne(oi => oi.Order)
				.WithMany(o => o.Items)
				.HasForeignKey(oi => oi.OrderId);

			modelBuilder.Entity( typeof(InventoryRecord) )
				.HasIndex("WarehouseId", "ProductId").IsUnique();

			modelBuilder.Entity<User>().HasIndex(u => u.Username).IsUnique();
		}
	}
}
