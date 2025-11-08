using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using Logistics.Api.Data;
using Logistics.Api.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Logistics.Api.Models;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

// 读取环境变量配置
builder.Configuration.AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: true);

builder.Services.AddControllersWithViews();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
{
	["Jwt:Key"] = "dev-secret-key-change-me-please",
	["Jwt:Issuer"] = "logistics.local",
	["Jwt:Audience"] = "logistics.local"
});

builder.Services.AddDbContext<ApplicationDbContext>(options =>
	options.UseInMemoryDatabase("LogisticsDb"));

// Explicit DI registrations
builder.Services.AddScoped<IWarehouseService, WarehouseService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IInventoryService, InventoryService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IShipmentService, ShipmentService>();
builder.Services.AddScoped<ITrackingService, TrackingService>();
builder.Services.AddScoped<IAuthService, AuthService>();

// AuthN/AuthZ
var key = Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!);
builder.Services.AddAuthentication(options =>
{
	options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
	options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
})
.AddCookie(options =>
{
	options.LoginPath = "/Admin/Login";
	options.AccessDeniedPath = "/Admin/AccessDenied";
	options.Cookie.Name = "LogisticsAuthCookie";
})
.AddJwtBearer(options =>
{
	options.TokenValidationParameters = new TokenValidationParameters
	{
		ValidateIssuer = true,
		ValidateAudience = true,
		ValidateIssuerSigningKey = true,
		ValidIssuer = builder.Configuration["Jwt:Issuer"],
		ValidAudience = builder.Configuration["Jwt:Audience"],
		IssuerSigningKey = new SymmetricSecurityKey(key)
	};
})
.AddCookie("StationCookies", options =>
{
	options.LoginPath = "/station/login";
	options.AccessDeniedPath = "/station/access-denied";
});

builder.Services.AddAuthorization(options =>
{
	options.AddPolicy("PlatformAdmin", p => p.RequireRole("PlatformAdmin"));
	options.AddPolicy("StationAdmin", p => p.RequireRole("StationAdmin"));
	options.AddPolicy("Employee", p => p.RequireRole("Employee"));
	options.AddPolicy("StationScoped", p => p.RequireAssertion(ctx =>
	{
		return ctx.User.IsInRole("StationAdmin") || ctx.User.IsInRole("Employee");
	}));
});

var app = builder.Build();

// Seed roles and a platform admin
using (var scope = app.Services.CreateScope())
{
	var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
	if (!db.Roles.Any())
	{
		db.Roles.AddRange(new Role { Name = "PlatformAdmin" }, new Role { Name = "StationAdmin" }, new Role { Name = "Employee" });
		await db.SaveChangesAsync();
	}
	if (!db.Users.Any())
	{
		var platformRoleId = db.Roles.First(r => r.Name == "PlatformAdmin").Id;
		var admin = new User { Username = "admin", PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123") ?? string.Empty };
		db.Users.Add(admin);
		db.UserRoles.Add(new UserRole { UserId = admin.Id, RoleId = platformRoleId });
		await db.SaveChangesAsync();
	}
}

if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Admin}/{action=Login}/{id?}");
app.MapControllers();

// 简单的错误处理中间件
app.Use(async (context, next) => {
    await next();
    if (context.Response.StatusCode == 404) {
        context.Request.Path = "/Error/HandleNotFound";
        await next();
    }
});

app.Run();
