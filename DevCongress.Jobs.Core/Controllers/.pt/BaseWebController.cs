using Microsoft.AspNetCore.Mvc;
using System.Web;
using System.Linq;
using System.Collections.Generic;
using Plutonium.Reactor.Services.Auth.User;
using DevCongress.Jobs.Core.Filters;

namespace DevCongress.Jobs.Core.Controllers
{
  [AutoValidateAntiforgeryToken]
  [ServiceFilter(typeof(AuthenticateActionFilter))]
  [AuthenticationExceptionFilter]
  public abstract partial class BaseWebController : BaseController
  {
    public AuthenticatedUser CurrentUser { get; internal set; }

    protected Dictionary<string, string> ReturnQueryToRouteValues(string query)
    {
      var routeValues = new Dictionary<string, string>();

      var parsed = HttpUtility.ParseQueryString(query ?? "");
      foreach (string key in parsed)
      {
        routeValues.Add(key, parsed[key]);
      }

      return routeValues;
    }
  }
}
