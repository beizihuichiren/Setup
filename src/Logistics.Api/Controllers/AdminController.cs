using System.Security.Claims;
using Logistics.Api.Data;
using Logistics.Api.Models;
using Logistics.Api.Models.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BCrypt.Net;

namespace Logistics.Api.Controllers
{
    public class AdminController(ApplicationDbContext db) : Controller
    {
        private readonly ApplicationDbContext _db = db;

        [Authorize(Roles = "PlatformAdmin")]
        public IActionResult Index()
        {
            // 创建模拟数据供视图使用
            var viewModel = new AdminDashboardViewModel()
            {
                TotalOrders = 1250,
                PendingOrders = 45,
                CompletedOrders = 1205,
                TotalRevenue = 358900.50m,
                TotalCustomers = 2350,
                TotalEmployees = 75,
                TotalStations = 12,
                RecentOrders = new List<OrderSummary>
                {
                    new OrderSummary { OrderId = "ORD-2024-001", CustomerName = "张三", Status = "已完成", Amount = 128.50m, CreatedTime = "2024-01-20 14:30" },
                    new OrderSummary { OrderId = "ORD-2024-002", CustomerName = "李四", Status = "处理中", Amount = 89.90m, CreatedTime = "2024-01-20 15:15" },
                    new OrderSummary { OrderId = "ORD-2024-003", CustomerName = "王五", Status = "待处理", Amount = 256.75m, CreatedTime = "2024-01-20 16:45" }
                },
                StationSummaries = new List<StationSummary>
                {
                    new StationSummary { StationId = "ST-001", StationName = "北京站点", PendingOrders = 12, TotalInventory = 3500, Status = "正常" },
                    new StationSummary { StationId = "ST-002", StationName = "上海站点", PendingOrders = 8, TotalInventory = 2800, Status = "正常" },
                    new StationSummary { StationId = "ST-003", StationName = "广州站点", PendingOrders = 15, TotalInventory = 4200, Status = "忙碌" }
                },
                MonthlyTrends = new List<TrendData>
                {
                    new TrendData { Period = "1月", OrderCount = 1250, Revenue = 358900.50m },
                    new TrendData { Period = "2月", OrderCount = 1120, Revenue = 325600.75m },
                    new TrendData { Period = "3月", OrderCount = 1380, Revenue = 389500.20m }
                }
            };
            return View(viewModel);
        }

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl ?? string.Empty;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(AdminLoginViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl ?? string.Empty;
            
            try
            {
                var user = await _db.Users.FirstOrDefaultAsync(u => u.Username != null && u.Username == model.Username);
                if (user == null || string.IsNullOrEmpty(user.PasswordHash) || !BCrypt.Net.BCrypt.Verify(model.Password, user.PasswordHash)) 
                    throw new InvalidOperationException("用户名或密码错误");
                
                var roleIds = await _db.UserRoles.Where(ur => ur.UserId == user.Id).Select(ur => ur.RoleId).ToListAsync();
                var roles = await _db.Roles.Where(r => roleIds.Contains(r.Id)).Select(r => r.Name).ToListAsync();
                
                if (!roles.Contains("PlatformAdmin")) 
                    throw new InvalidOperationException("非管理员账号，无法登录管理后台");

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Name, user.Username ?? string.Empty),
                    new Claim(ClaimTypes.Role, "PlatformAdmin")
                };
                
                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme, ClaimTypes.Name, ClaimTypes.Role);
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));
                
                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl ?? string.Empty))
                    return Redirect(returnUrl ?? string.Empty);
                
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(model);
            }
        }

        [Authorize(Roles = "PlatformAdmin")]
        public IActionResult Orders()
        {
            return View();
        }

        [Authorize(Roles = "PlatformAdmin")]
        public IActionResult Users()
        {
            return View();
        }

        [Authorize(Roles = "PlatformAdmin")]
        public IActionResult Products()
        {
            return View();
        }

        [Authorize(Roles = "PlatformAdmin")]
        public IActionResult Warehouses()
        {
            return View();
        }

        [Authorize(Roles = "PlatformAdmin")]
        public IActionResult Stations()
        {
            return View();
        }

        [Authorize(Roles = "PlatformAdmin")]
        public IActionResult AfterSales()
        {
            return View();
        }

        [Authorize(Roles = "PlatformAdmin")]
        public IActionResult Tracking()
        {
            return View();
        }

        public IActionResult Logout()
        {
            HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme).Wait();
            return RedirectToAction("Login");
        }

        /// <summary>
        /// 处理访问被拒绝的情况
        /// </summary>
        public IActionResult AccessDenied()
        {
            ViewData["Title"] = "访问被拒绝";
            return View();
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(AdminRegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // 检查用户是否已存在
                    if (await _db.Users.AnyAsync(u => u.Username == model.Username))
                    {
                        ModelState.AddModelError("Username", "用户名已存在");
                        return View(model);
                    }

                    // 获取PlatformAdmin角色
                    var role = await _db.Roles.FirstOrDefaultAsync(r => r.Name == "PlatformAdmin");
                    if (role == null)
                    {
                        ModelState.AddModelError(string.Empty, "系统角色配置错误");
                        return View(model);
                    }

                    // 创建新用户
                    var user = new User
                    {
                        Username = model.Username,
                        PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password)
                    };

                    _db.Users.Add(user);
                    await _db.SaveChangesAsync();

                    // 分配角色
                    _db.UserRoles.Add(new UserRole { UserId = user.Id, RoleId = role.Id });
                    await _db.SaveChangesAsync();

                    return RedirectToAction("Login", new { message = "注册成功，请登录" });
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty, "注册失败：" + ex.Message);
                }
            }
            return View(model);
        }
    }
}