using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace Logistics.Api.Controllers
{
    public class ErrorController : Controller
    {
        /// <summary>
        /// 处理404未找到错误
        /// </summary>
        /// <returns>404错误视图</returns>
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult HandleNotFound()
        {
            ViewData["Title"] = "页面未找到";
            return View("/Views/Shared/404.cshtml");
        }

        /// <summary>
        /// 处理其他类型的错误
        /// </summary>
        /// <param name="statusCode">HTTP状态码</param>
        /// <returns>通用错误视图</returns>
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Index(int? statusCode = null)
        {
            ViewData["StatusCode"] = statusCode ?? 500;
            ViewData["Title"] = "发生错误";
            return View("/Views/Shared/404.cshtml");
        }
    }
}