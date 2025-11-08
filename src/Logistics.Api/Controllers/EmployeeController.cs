using System.Security.Claims;
using Logistics.Api.Data;
using Logistics.Api.Models;
using Logistics.Api.Models.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Logistics.Api.Controllers
{
    public class EmployeeController : Controller
    {
        private readonly ApplicationDbContext _db;
        
        public EmployeeController(ApplicationDbContext db)
        {
            _db = db;
        }

        [Authorize(Roles = "Employee")]
        public IActionResult Index()
        {
            // 创建模拟数据供视图使用
            var viewModel = new EmployeeDashboardViewModel
            {
                PendingOrders = 8,
                ProcessingOrders = 12,
                TodayCompleted = 5,
                MonthlyEarnings = 8500.00m,
                RecentOrders = new List<EmployeeOrderSummary>
                {
                    new EmployeeOrderSummary { OrderId = "ORD-2024-001", CustomerInfo = "张三 (138****1234)", Status = "待配送", DueTime = "今天 17:00前", ActionUrl = "/Employee/Orders" },
                    new EmployeeOrderSummary { OrderId = "ORD-2024-002", CustomerInfo = "李四 (139****5678)", Status = "配送中", DueTime = "今天 18:30前", ActionUrl = "/Employee/Orders" },
                    new EmployeeOrderSummary { OrderId = "ORD-2024-003", CustomerInfo = "王五 (137****9012)", Status = "待取货", DueTime = "今天 16:00前", ActionUrl = "/Employee/Orders" }
                },
                Reminders = new List<WorkReminder>
                {
                    new WorkReminder { Id = "R-001", Title = "客户回访", Description = "请与客户张小姐联系确认服务满意度", Priority = "高", DueTime = "今天 16:30" },
                    new WorkReminder { Id = "R-002", Title = "系统培训", Description = "参加新系统操作培训会议", Priority = "中", DueTime = "明天 14:00" },
                    new WorkReminder { Id = "R-003", Title = "绩效提交", Description = "提交本月工作绩效报告", Priority = "中", DueTime = "明天 17:00" }
                },
                PerformanceData = new List<PerformanceData>
                {
                    new PerformanceData { Period = "第1周", CompletedOrders = 45, Earnings = 1800.00m },
                    new PerformanceData { Period = "第2周", CompletedOrders = 52, Earnings = 2080.00m },
                    new PerformanceData { Period = "第3周", CompletedOrders = 48, Earnings = 1920.00m },
                    new PerformanceData { Period = "第4周", CompletedOrders = 35, Earnings = 1400.00m }
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
                
                if (!roles.Contains("Employee")) 
                    throw new InvalidOperationException("非员工账号，无法登录员工后台");

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Name, user.Username ?? string.Empty),
                    new Claim(ClaimTypes.Role, "Employee")
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

        [Authorize(Roles = "Employee")]
        public IActionResult Orders()
        {
            return View();
        }

        [Authorize(Roles = "Employee")]
        public IActionResult Operations()
        {
            return View();
        }

        [Authorize(Roles = "Employee")]
        public IActionResult Earnings()
        {
            return View();
        }

        [Authorize(Roles = "Employee")]
        public IActionResult Profile()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
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

                    // 获取Employee角色
                    var role = await _db.Roles.FirstOrDefaultAsync(r => r.Name == "Employee");
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

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }
    }

    // 视图模型
    public class LoginViewModel
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class RegisterViewModel
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string ConfirmPassword { get; set; } = string.Empty;
        // 可以添加其他注册字段
    }
}