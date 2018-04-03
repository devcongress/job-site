using Enexure.MicroBus;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using DevCongress.Jobs.Core.Features.Auth.Password.Login;
using System.Threading.Tasks;

namespace DevCongress.Jobs.Core.Controllers.Web
{
  public partial class AuthApiController : BaseAuthApiController
  {
    public AuthApiController(ILogger<AuthApiController> logger, IMicroBus microBus) : base(logger, microBus)
    {
    }
  }

  #region Double-Derived

  [Route("Api")]
  public abstract class BaseAuthApiController : BaseApiController
  {
    protected readonly ILogger<AuthApiController> _logger;
    protected readonly IMicroBus _microBus;

    protected BaseAuthApiController(ILogger<AuthApiController> logger, IMicroBus microBus)
    {
      _logger = logger;
      _microBus = microBus;
    }

    [HttpPost("Login")]
    public virtual async Task<IActionResult> Login(
        [FromForm] string Username,
        [FromForm] string Password
    )
    {
      var query = new PasswordLoginQuery(
                  Username: Username,
                  Password: Password);
      var res = await _microBus.QueryAsync(query);

      return Json(res);
    }
  }

  #endregion Double-Derived
}
