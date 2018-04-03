using Enexure.MicroBus;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Newtonsoft.Json;
using DevCongress.Jobs.Core.Controllers;
using Plutonium.Reactor.Features.User.Fetch;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace DevCongress.Jobs.Core.Filters
{
  [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
  public class AuthenticateActionFilter : ActionFilterAttribute
  {
    private readonly IMicroBus _microBus;

    public AuthenticateActionFilter(
        IMicroBus microBus
    )
    {
      _microBus = microBus;
    }

    public override void OnActionExecuting(ActionExecutingContext context)
    {
      OnActionExecutingInternal(context).Wait();
    }

    private async Task OnActionExecutingInternal(ActionExecutingContext context)
    {
      if (context.Controller is BaseWebController controller && context.ActionDescriptor is ControllerActionDescriptor controllerActionDescriptor)
      {
        var query = new GetAuthenticatedUserQuery();
        var res = await _microBus.QueryAsync(query).ConfigureAwait(false);

        controller.CurrentUser = res.User;
        controller.ViewData["CurrentUser"] = res.User;

        if (res.User.IsAnonymous)
        {
          var allowAnon = controllerActionDescriptor.MethodInfo.GetCustomAttributes(typeof(AllowAnonymousAttribute), inherit: false);
          if (!allowAnon.Any())
          {
            controller.AddError("Please login first");
            context.Result = new RedirectToRouteResult("login", null);
          }
        }
      }
    }
  }
}
