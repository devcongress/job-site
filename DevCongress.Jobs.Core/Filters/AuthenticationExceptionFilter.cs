using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Plutonium.Reactor.Pipeline.Crosscutting.Auth.Exception;
using System;

namespace DevCongress.Jobs.Core.Filters
{
    [AttributeUsage(AttributeTargets.All, AllowMultiple = false, Inherited = true)]
    public class AuthenticationExceptionFilter : ExceptionFilterAttribute
    {
        public override void OnException(ExceptionContext context)
        {
            if (context.Exception is UnAuthenticatedUserAgentException)
                context.Result = new RedirectToRouteResult("login", null);
            else if (context.Exception is UnAuthorizedRequestException)
                context.Result = new StatusCodeResult(403);
        }
    }
}