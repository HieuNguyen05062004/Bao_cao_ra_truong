using Client.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Client.Controllers;

/// <summary>
/// Base controller cho Client app - tự động set ViewBag user info
/// </summary>
public abstract class ClientBaseController : Controller
{
    public override void OnActionExecuted(ActionExecutedContext context)
    {
        // Set user info từ session vào ViewBag để sử dụng trong view
        var readerId = HttpContext.Session.GetReaderId();
        var readerName = HttpContext.Session.GetReaderName();
        var readerAvatar = HttpContext.Session.GetString("ReaderAvatar");

        ViewBag.IsLoggedIn = !string.IsNullOrEmpty(readerId);
        ViewBag.ReaderId = readerId;
        ViewBag.ReaderName = readerName;
        ViewBag.ReaderAvatar = readerAvatar;

        base.OnActionExecuted(context);
    }
}
