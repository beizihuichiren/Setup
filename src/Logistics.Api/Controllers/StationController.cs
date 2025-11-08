using System.Security.Claims;
using Logistics.Api.Data;
using Logistics.Api.Models.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Logistics.Api.Controllers
{
    public class StationController : Controller
    {
        private readonly ApplicationDbContext _db;
        
        public StationController(ApplicationDbContext db)
        {
            _db = db;
        }

        [Authorize(Roles = "Station")]
        public IActionResult Index()
        {
            // 创建模拟数据供视图使用
            var viewModel = new StationDashboardViewModel
            {
                TodayOrders = 25,
                LowInventoryAlert = 3,
                AfterSalesTickets = 7,
                InventoryTurnover = 12.5m,
                PendingOrders = new List<StationOrderSummary>
                {
                    new StationOrderSummary { OrderId = "ORD-2024-001", CustomerName = "张三", ProductCount = "3件", CreatedTime = "10:30", ActionUrl = "/Station/Orders" },
                    new StationOrderSummary { OrderId = "ORD-2024-002", CustomerName = "李四", ProductCount = "2件", CreatedTime = "10:45", ActionUrl = "/Station/Orders" },
                    new StationOrderSummary { OrderId = "ORD-2024-003", CustomerName = "王五", ProductCount = "5件", CreatedTime = "11:15", ActionUrl = "/Station/Orders" }
                },
                LowInventoryProducts = new List<LowInventoryProduct>
                {
                    new LowInventoryProduct { ProductId = "P-001", ProductName = "一次性口罩", CurrentStock = 45, AlertThreshold = 100, Unit = "个" },
                    new LowInventoryProduct { ProductId = "P-002", ProductName = "消毒湿巾", CurrentStock = 30, AlertThreshold = 50, Unit = "包" },
                    new LowInventoryProduct { ProductId = "P-003", ProductName = "防护服", CurrentStock = 12, AlertThreshold = 20, Unit = "套" }
                },
                PendingAfterSalesTickets = new List<AfterSalesTicketSummary>
                {
                    new AfterSalesTicketSummary { TicketId = "AS-001", CustomerInfo = "张三 (138****1234)", Type = "退货", CreatedTime = "昨天 15:30", ActionUrl = "/Station/AfterSales" },
                    new AfterSalesTicketSummary { TicketId = "AS-002", CustomerInfo = "李四 (139****5678)", Type = "换货", CreatedTime = "今天 09:15", ActionUrl = "/Station/AfterSales" },
                    new AfterSalesTicketSummary { TicketId = "AS-003", CustomerInfo = "王五 (137****9012)", Type = "投诉", CreatedTime = "今天 11:00", ActionUrl = "/Station/AfterSales" }
                },
                WeeklyTrends = new List<WeeklyTrendData>
                {
                    new WeeklyTrendData { Day = "周一", OrderCount = 32, CompletedCount = 30 },
                    new WeeklyTrendData { Day = "周二", OrderCount = 28, CompletedCount = 26 },
                    new WeeklyTrendData { Day = "周三", OrderCount = 40, CompletedCount = 35 },
                    new WeeklyTrendData { Day = "周四", OrderCount = 35, CompletedCount = 32 },
                    new WeeklyTrendData { Day = "周五", OrderCount = 42, CompletedCount = 38 },
                    new WeeklyTrendData { Day = "周六", OrderCount = 25, CompletedCount = 20 },
                    new WeeklyTrendData { Day = "周日", OrderCount = 20, CompletedCount = 18 }
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
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl ?? string.Empty;
            
            try
            {
                var user = await _db.Users.FirstOrDefaultAsync(u => u.Username != null && u.Username == model.Username);
                if (user == null || !BCrypt.Net.BCrypt.Verify(model.Password, user.PasswordHash ?? string.Empty)) 
                    throw new InvalidOperationException("用户名或密码错误");
                
                var roleIds = await _db.UserRoles.Where(ur => ur.UserId == user.Id).Select(ur => ur.RoleId).ToListAsync();
                var roles = await _db.Roles.Where(r => roleIds.Contains(r.Id)).Select(r => r.Name).ToListAsync();
                
                if (!roles.Contains("Station")) 
                    throw new InvalidOperationException("非站点账号，无法登录站点后台");

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Name, user.Username ?? string.Empty),
                    new Claim(ClaimTypes.Role, "Station")
                };
                
                if (user.StationId.HasValue) 
                    claims.Add(new Claim("station_id", user.StationId.Value.ToString()));
                
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

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                // 这里可以添加注册逻辑
                return RedirectToAction("Login");
            }
            return View(model);
        }

        [Authorize(Roles = "Station")]
        public IActionResult Inventory()
        {
            return View();
        }

        [Authorize(Roles = "Station")]
        public IActionResult Orders()
        {
            return View();
        }

        [Authorize(Roles = "Station")]
        public IActionResult Products()
        {
            return View();
        }

        [Authorize(Roles = "Station")]
        public IActionResult AfterSales()
        {
            return View();
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }
    }
}