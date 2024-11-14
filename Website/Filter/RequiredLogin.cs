using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using RSA_UI.Utils;
using RSA_UI.Controllers;

namespace RSA_UI.Filter;

public class RequiredLogin : ActionFilterAttribute
{
    private readonly string _authControllerName = nameof(AuthController).Replace("Controller", "");
    private readonly string _homeControllerName = nameof(HomeController).Replace("Controller", "");

    public override void OnActionExecuting(ActionExecutingContext context)
    {
        var uuid = context.HttpContext.Session.GetString("uuidCompany");
        bool hasLogin = uuid.IsNotNullOrEmpty();
        string controllerName = context.ActionDescriptor.RouteValues["controller"] ?? "";

        bool isAuthController = controllerName.Equals(_authControllerName, StringComparison.OrdinalIgnoreCase);
        bool isHomeController = controllerName.Equals(_homeControllerName, StringComparison.OrdinalIgnoreCase);

        if (!hasLogin && !isAuthController)
        {
            context.HttpContext.Session.Clear();
            context.Result = new RedirectToActionResult(nameof(AuthController.Index), _authControllerName, null);
            return;
        }

        if (hasLogin && isAuthController)
        {
            context.Result = new RedirectToActionResult(nameof(HomeController.Index), _homeControllerName, null);
            return;
        }
    }
}
