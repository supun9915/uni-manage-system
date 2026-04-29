using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace UniManage.Controllers
{
    public class BaseController : Controller
    {
        protected int? CurrentUserId => HttpContext.Session.GetInt32("UserId");
        protected string? CurrentUserName => HttpContext.Session.GetString("UserName");
        protected string? CurrentUserRole => HttpContext.Session.GetString("UserRole");
        protected int? CurrentRoleId => HttpContext.Session.GetInt32("RoleId");

        protected bool IsAdmin => string.Equals(CurrentUserRole, "administrator", StringComparison.OrdinalIgnoreCase);
        protected bool IsLecturer => string.Equals(CurrentUserRole, "lecturer", StringComparison.OrdinalIgnoreCase);
        protected bool IsStudent => string.Equals(CurrentUserRole, "student", StringComparison.OrdinalIgnoreCase);

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (CurrentUserId == null)
            {
                context.Result = RedirectToAction("Login", "Account");
                return;
            }
            ViewBag.CurrentUserId = CurrentUserId;
            ViewBag.CurrentUserName = CurrentUserName;
            ViewBag.CurrentUserRole = CurrentUserRole?.ToLower();
            base.OnActionExecuting(context);
        }
    }
}
