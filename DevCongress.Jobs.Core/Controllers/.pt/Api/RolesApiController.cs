using Enexure.MicroBus;
using FluentResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using DevCongress.Jobs.Core.Features.Role.Add;
using DevCongress.Jobs.Core.Features.Role.Fetch;
using DevCongress.Jobs.Core.Features.Role.GetLogs;
using DevCongress.Jobs.Core.Features.Role.List;
using DevCongress.Jobs.Core.Features.Role.Restore;
using DevCongress.Jobs.Core.Features.Role.Trash;
using DevCongress.Jobs.Core.Features.Role.Update;
using DevCongress.Jobs.Core.Features.Role.UpdatePermissions;
using Plutonium.Reactor.Data.Query.Filter;
using Plutonium.Reactor.Data.Query.Sort;
using System;
using System.Threading.Tasks;
using static DevCongress.Jobs.Core.Domain.Model.Role;

namespace DevCongress.Jobs.Core.Controllers.Api
{
  public partial class RolesApiController : BaseRolesApiController
  {
    public RolesApiController(ILogger<RolesApiController> logger, IMicroBus microBus) : base(logger, microBus)
    {
    }
  }

  #region Double-Derived

  [Route("Api/Roles")]
  public abstract class BaseRolesApiController : BaseApiController
  {
    protected readonly ILogger<RolesApiController> _logger;
    protected readonly IMicroBus _microBus;

    protected BaseRolesApiController(ILogger<RolesApiController> logger, IMicroBus microBus)
    {
      _logger = logger;
      _microBus = microBus;
    }

    [HttpGet]
    public virtual async Task<IActionResult> List(
      [FromQuery] int Page = 1
      , [FromQuery] int Limit = 10
      , string Filter = "all"
      , DateFilter CreateDateFilter = default(DateFilter)
      , DateFilter UpdateDateFilter = default(DateFilter)
      , DateFilter DeleteDateFilter = default(DateFilter)
      , string SortBy = "id"
      , string SortOrder = "asc"
      , [FromQuery] bool IncludeTrashed = false
    )
    {
      Enum.TryParse(Filter, true, out RoleFilter filter);

      Enum.TryParse(SortBy, true, out RoleColumn sortBy);
      Enum.TryParse(SortOrder, true, out SortDirection sortOrder);
      var sort = new SortBy<RoleColumn>(sortBy, sortOrder);

      var query = new ListRolesQuery(Page, Limit, filter, CreateDateFilter, UpdateDateFilter, DeleteDateFilter, sort, IncludeTrashed);
      var res = await _microBus.QueryAsync(query);

      return Json(res);
    }

    [HttpGet("{Id}")]
    public virtual async Task<IActionResult> Fetch([FromRoute] int Id)
    {
      var query = new FetchRoleQuery(Id);
      var res = await _microBus.QueryAsync(query);

      return Json(res);
    }

    [HttpPost]
    public virtual async Task<IActionResult> Add(
            [FromForm] string Name,
            [FromForm] string Description,
            [FromForm] bool IsActive = true,
            string _ = null // used as a dummy until i fix the trailing comma issue
    )
    {
      var result = new TaskCompletionSource<Result>();
      var command = new AddRoleCommand(
                Name: Name,
                Description: Description,
                IsActive: IsActive,

        result: result);

      await _microBus.SendAsync(command);
      var res = await result.Task;

      return Json(res);
    }

    [HttpPut("{Id}")]
    public virtual async Task<IActionResult> Update(
      [FromRoute] int Id,
            [FromForm] string Name,
            [FromForm] string Description,
            [FromForm] bool IsActive = true,
            string _ = null // used as a dummy until i fix the trailing comma issue
    )
    {
      var result = new TaskCompletionSource<Result>();
      var command = new UpdateRoleCommand(
        id: Id,
            Name: Name,
            Description: Description,
            IsActive: IsActive,

      result: result);

      await _microBus.SendAsync(command);
      var res = await result.Task;

      return Json(res);
    }

    [HttpPut("{Id}/Permissions")]
    public virtual async Task<IActionResult> EditPermissions(
        [FromRoute] int Id,
        [FromForm] int[] Permissions
    )
    {
      var result = new TaskCompletionSource<Result>();
      var command = new UpdateRolePermissionsCommand(
          id: Id,
          Permissions: Permissions,

          result: result);

      await _microBus.SendAsync(command);
      var res = await result.Task;

      return Json(res);
    }

    [HttpDelete("{Id}")]
    public virtual async Task<IActionResult> Trash([FromRoute] int Id)
    {
      var result = new TaskCompletionSource<Result>();
      var command = new TrashRolesCommand(new int[] { Id }, result);

      await _microBus.SendAsync(command);
      var res = await result.Task;

      return Json(res);
    }

    [HttpPost("{Id}/Restore")]
    public virtual async Task<IActionResult> Restore([FromRoute] int Id)
    {
      var result = new TaskCompletionSource<Result>();
      var command = new RestoreRolesCommand(new int[] { Id }, result);

      await _microBus.SendAsync(command);
      var res = await result.Task;

      return Json(res);
    }

    [HttpGet("{Id}/Logs")]
    public virtual async Task<IActionResult> GetLogs([FromRoute] int Id, [FromQuery] int Limit = 50)
    {
      var query = new GetRoleLogsQuery(Id, Limit);
      var res = await _microBus.QueryAsync(query);

      return Json(res);
    }
  }

  #endregion Double-Derived
}
