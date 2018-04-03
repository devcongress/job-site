using Enexure.MicroBus;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace DevCongress.Jobs.Core.Controllers
{
  public partial class DashboardController : BaseDashboardController
  {
    public DashboardController(
        ILogger<DashboardController> logger,
        IMicroBus microBus) : base(logger, microBus)
    {
    }
  }

  #region Double-Derived

  public abstract class BaseDashboardController : BaseWebController
  {
    protected readonly ILogger<DashboardController> _logger;
    protected readonly IMicroBus _microBus;

    protected BaseDashboardController(
        ILogger<DashboardController> logger,
        IMicroBus microBus)
    {
      _logger = logger;
      _microBus = microBus;
    }

    [HttpGet("Dashboard", Name = "dashboard")]
    public virtual Task<IActionResult> Index()
    {
      return Task.FromResult((IActionResult)View());
    }
  }

  #endregion Double-Derived
}
