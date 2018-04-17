using Enexure.MicroBus;
using FluentResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using DevCongress.Jobs.Core.Features.User.Add;
using DevCongress.Jobs.Core.Features.User.Fetch;
using DevCongress.Jobs.Core.Features.User.GetLogs;
using DevCongress.Jobs.Core.Features.User.List;
using DevCongress.Jobs.Core.Features.User.Restore;
using DevCongress.Jobs.Core.Features.User.SetUserPassword;
using DevCongress.Jobs.Core.Features.User.Trash;
using DevCongress.Jobs.Core.Features.User.Update;
using DevCongress.Jobs.Core.Features.User.UpdateProfile;
using DevCongress.Jobs.Core.Features.User.UpdateRoles;
using Plutonium.Reactor.Data.Query.Filter;
using Plutonium.Reactor.Data.Query.Sort;
using System;
using System.Threading.Tasks;
using static DevCongress.Jobs.Core.Domain.Model.User;


namespace DevCongress.Jobs.Core.Controllers.Api
{
  public partial class UsersApiController : BaseUsersApiController
  {
    public UsersApiController(ILogger<UsersApiController> logger, IMicroBus microBus) : base(logger, microBus)
    {
    }
  }

  #region Double-Derived

  [Route("Api/Users")]
  public abstract class BaseUsersApiController : BaseApiController
  {
    protected readonly ILogger<UsersApiController> _logger;
    protected readonly IMicroBus _microBus;

    protected BaseUsersApiController(ILogger<UsersApiController> logger, IMicroBus microBus)
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
      Enum.TryParse(Filter, true, out UserFilter filter);

      Enum.TryParse(SortBy, true, out UserColumn sortBy);
      Enum.TryParse(SortOrder, true, out SortDirection sortOrder);
      var sort = new SortBy<UserColumn>(sortBy, sortOrder);

      var query = new ListUsersQuery(Page, Limit, filter, CreateDateFilter, UpdateDateFilter, DeleteDateFilter, sort, IncludeTrashed);
      var res = await _microBus.QueryAsync(query);

      return Json(res);
    }

    [HttpGet("{Id}")]
    public virtual async Task<IActionResult> Fetch([FromRoute] int Id)
    {
      var query = new FetchUserQuery(Id);
      var res = await _microBus.QueryAsync(query);

      return Json(res);
    }

    [HttpPost]
    public virtual async Task<IActionResult> Add(
            [FromForm] int? TenantId,
            [FromForm] string Username,
            [FromForm] string Password,
            [FromForm] string ConfirmPassword
    )
    {
      var result = new TaskCompletionSource<Result>();
      var command = new AddUserCommand(
                TenantId: TenantId,
                Username: Username,
                Password: Password,
                ConfirmPassword: ConfirmPassword,

        result: result);

      await _microBus.SendAsync(command);
      var res = await result.Task;

      return Json(res);
    }

    [HttpPut("{Id}")]
    public virtual async Task<IActionResult> Update(
      [FromRoute] int Id,
            [FromForm] string Username,
            [FromForm] DateTimeOffset? BanEnd = null,
            [FromForm] bool IsConfirmed = false,
            [FromForm] bool IsActive = true
    )
    {
      var result = new TaskCompletionSource<Result>();
      var command = new UpdateUserCommand(
        id: Id,
            Username: Username,
            BanEnd: BanEnd,
            IsConfirmed: IsConfirmed,
            IsActive: IsActive,

      result: result);

      await _microBus.SendAsync(command);
      var res = await result.Task;

      return Json(res);
    }

    [HttpPut("{Id}/Profile")]
    public virtual async Task<IActionResult> EditProfile(
        [FromRoute] int Id
      , [FromForm] string Name
        , [FromForm] string CompanyEmail = null
        , [FromForm] string CompanyWebsite = null
        , [FromForm] string CompanyTwitter = null
        , [FromForm] string CompanyDescription = null
        )
    {
      var result = new TaskCompletionSource<Result>();
      var command = new UpdateUserProfileCommand(
          UserId: Id,
          Name: Name,
                    CompanyEmail : CompanyEmail,
                    CompanyWebsite : CompanyWebsite,
                    CompanyTwitter : CompanyTwitter,
                    CompanyDescription : CompanyDescription,
          

          result: result);

      await _microBus.SendAsync(command);
      var res = await result.Task;

      return Json(res);
    }

    [HttpPut("{Id}/Roles")]
    public virtual async Task<IActionResult> EditRoles(
        [FromRoute] int Id,
        [FromForm] int[] Roles
    )
    {
      var result = new TaskCompletionSource<Result>();
      var command = new UpdateUserRolesCommand(
          id: Id,
          RoleIds: Roles,

          result: result);

      await _microBus.SendAsync(command);
      var res = await result.Task;

      return Json(res);
    }

    [HttpPut("{Id}/Password")]
    public virtual async Task<IActionResult> ChangePassword(
      [FromRoute] int Id,
            [FromForm] string Password,
            [FromForm] string ConfirmPassword
    )
    {
      var result = new TaskCompletionSource<Result>();
      var command = new SetUserPasswordCommand(
        id: Id,
            Password: Password,
            ConfirmPassword: ConfirmPassword,

      result: result);

      await _microBus.SendAsync(command);
      var res = await result.Task;

      return Json(res);
    }

    [HttpDelete("{Id}")]
    public virtual async Task<IActionResult> Trash([FromRoute] int Id)
    {
      var result = new TaskCompletionSource<Result>();
      var command = new TrashUsersCommand(new int[] { Id }, result);

      await _microBus.SendAsync(command);
      var res = await result.Task;

      return Json(res);
    }

    [HttpPost("{Id}/Restore")]
    public virtual async Task<IActionResult> Restore([FromRoute] int Id)
    {
      var result = new TaskCompletionSource<Result>();
      var command = new RestoreUsersCommand(new int[] { Id }, result);

      await _microBus.SendAsync(command);
      var res = await result.Task;

      return Json(res);
    }

    [HttpGet("{Id}/Logs")]
    public virtual async Task<IActionResult> GetLogs([FromRoute] int Id, [FromQuery] int Limit = 50)
    {
      var query = new GetUserLogsQuery(Id, Limit);
      var res = await _microBus.QueryAsync(query);

      return Json(res);
    }
  }

  #endregion Double-Derived
}
