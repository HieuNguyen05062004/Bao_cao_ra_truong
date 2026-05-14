using Client.Extensions;
using Core.Shared.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Client.Controllers;

/// <summary>
/// Base controller cho Client app — tự động set ViewBag và validate session còn hợp lệ
/// </summary>
public abstract class ClientBaseController : Controller
{
    public override void OnActionExecuted(ActionExecutedContext context)
    {
        var readerId = HttpContext.Session.GetReaderId();
        var readerName = HttpContext.Session.GetReaderName();
        var readerAvatar = HttpContext.Session.GetString("ReaderAvatar");

        ViewBag.IsLoggedIn = !string.IsNullOrEmpty(readerId);
        ViewBag.ReaderId = readerId;
        ViewBag.ReaderName = readerName;
        ViewBag.ReaderAvatar = readerAvatar;

        base.OnActionExecuted(context);
    }

    public override async Task OnActionExecutionAsync(
        ActionExecutingContext context,
        ActionExecutionDelegate next)
    {
        var readerId = HttpContext.Session.GetString("ReaderId");

        // Nếu có session ReaderId, kiểm tra reader còn tồn tại trong DB không
        if (!string.IsNullOrEmpty(readerId))
        {
            var readerService = HttpContext.RequestServices
                .GetService<IReaderService>();

            if (readerService != null)
            {
                var exists = await readerService.ReaderIdExistsAsync(readerId);
                if (!exists)
                {
                    // Reader đã bị xóa → clear session + xóa cookie
                    HttpContext.Session.Clear();
                    foreach (var cookie in Request.Cookies.Keys)
                        Response.Cookies.Delete(cookie);

                    // Nếu đang ở trang cần đăng nhập thì redirect về Login
                    // Nếu ở trang public (Home, Search) thì vẫn cho xem
                    context.Result = new RedirectToActionResult("Login", "Account", null);
                    return;
                }
            }
        }

        await next();
    }
}
