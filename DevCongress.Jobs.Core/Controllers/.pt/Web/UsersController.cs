using Enexure.MicroBus;
using FluentResults;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using DevCongress.Jobs.Core.Features.Role.List;
using DevCongress.Jobs.Core.Features.User.Add;
using DevCongress.Jobs.Core.Features.User.Fetch;
using DevCongress.Jobs.Core.Features.User.List;
using DevCongress.Jobs.Core.Features.User.Restore;
using DevCongress.Jobs.Core.Features.User.SetUserPassword;
using DevCongress.Jobs.Core.Features.User.Trash;
using DevCongress.Jobs.Core.Features.User.Update;
using DevCongress.Jobs.Core.Features.User.UpdateProfile;
using DevCongress.Jobs.Core.Features.User.UpdateRoles;
using DevCongress.Jobs.Core.ViewModels.Users;
using Plutonium.Reactor.Data.Query.Filter;
using Plutonium.Reactor.Data.Query.Sort;
using System;
using System.Linq;
using System.Threading.Tasks;
using static DevCongress.Jobs.Core.Domain.Model.User;


namespace DevCongress.Jobs.Core.Controllers.Web
{
  public partial class UsersController : BaseUsersController
  {
    public UsersController(ILogger<UsersController> logger, IMicroBus microBus) : base(logger, microBus)
    {
    }
  }

  #region Double-Derived

  [Route("Users")]
  public abstract class BaseUsersController : BaseWebController
  {
    protected readonly ILogger<UsersController> _logger;
    protected readonly IMicroBus _microBus;

    protected BaseUsersController(ILogger<UsersController> logger, IMicroBus microBus)
    {
      _logger = logger;
      _microBus = microBus;
    }

    [HttpGet(Name = "list_users")]
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

      var vm = new ListUsersViewModel()
      {
        Page = Page,
        Limit = Limit,
        SortBy = SortBy,
        SortOrder = SortOrder,
        IncludeTrashed = IncludeTrashed,
      };

      try
      {
        var query = new ListUsersQuery(Page, Limit, filter, CreateDateFilter, UpdateDateFilter, DeleteDateFilter, sort, IncludeTrashed);
        var res = await _microBus.QueryAsync(query);

        if (res.IsSuccess)
        {
          vm = new ListUsersViewModel()
          {
            Users = res.Users,

            Page = res.Page,
            TotalPageCount = res.TotalPageCount,

            TotalCount = res.TotalCount,
            Limit = res.Limit,

            SortBy = SortBy,
            SortOrder = SortOrder,

            IncludeTrashed = IncludeTrashed,

            IsSuccess = res.IsSuccess,
          };
        }
        else
        {
          AddError(res.Errors);
        }
      }
      catch (ValidationException ve)
      {
        AddError(ve);
      }

      return View(vm);
    }

    [HttpGet("{Id}", Name = "view_user")]
    public virtual async Task<IActionResult> View([FromRoute] int Id)
    {
      var vm = new ViewUserViewModel();

      try
      {
        var query = new FetchUserQuery(Id, includeLogs: true);
        var res = await _microBus.QueryAsync(query);

        if (res.IsSuccess)
        {
          vm = new ViewUserViewModel()
          {
            User = res.User,
            AuditLogs = res.AuditLogs,

            IsSuccess = res.IsSuccess,
          };
        }
        else
        {
          AddError(res.Errors);
        }
      }
      catch (ValidationException ve)
      {
        AddError(ve);
      }

      return View(vm);
    }

    [HttpGet("Add", Name = "add_user")]
    public IActionResult Add()
    {
      return View(new AddUserViewModel());
    }

    [HttpPost("Add", Name = "add_user")]
    public virtual async Task<IActionResult> Add(
            [FromForm] int? TenantId,
            [FromForm] string Username,
            [FromForm] string Password,
            [FromForm] string ConfirmPassword
    )
    {
      var vm = new AddUserViewModel()
      {
        Username = Username,
        Password = Password,
        ConfirmPassword = ConfirmPassword,
      };

      try
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

        if (res.IsSuccess)
        {
          AddSuccess(res.Successes);
          return RedirectToRoute("list_users");
        }
        else
        {
          AddError(res.Errors);
        }
      }
      catch (ValidationException ve)
      {
        AddError(ve);
      }

      return View(vm);
    }

    [HttpGet("{Id}/Edit", Name = "edit_user")]
    public virtual async Task<IActionResult> Edit([FromRoute] int Id)
    {
      var vm = new EditUserViewModel()
      {
      };

      try
      {
        var query = new FetchUserQuery(Id);
        var res = await _microBus.QueryAsync(query);

        if (res.IsSuccess)
        {
          vm = new EditUserViewModel()
          {
            User = res.User,

            IsSuccess = res.IsSuccess,
          };

          var rolesQuery = new ListRolesQuery(limit: 0, sortBy: new SortBy<Domain.Model.Role.RoleColumn>(Domain.Model.Role.RoleColumn.Name, SortDirection.Asc));
          var rolesRes = await _microBus.QueryAsync(rolesQuery);

          if (rolesRes.IsSuccess)
          {
            vm.Roles = rolesRes.Roles.Where(r => r.IsActive).ToArray();
          }
          else
          {
            AddError(res.Errors);
          }
        }
        else
        {
          AddError(res.Errors);
        }
      }
      catch (ValidationException ve)
      {
        AddError(ve);
      }

      return View(vm);
    }

    [HttpPost("{Id}/Edit", Name = "edit_user")]
    public virtual async Task<IActionResult> Edit(
      [FromRoute] int Id,
            [FromForm] string Username,
            [FromForm] DateTimeOffset? BanEnd = null,
            [FromForm] bool IsConfirmed = false,
            [FromForm] bool IsActive = true
    )
    {
      try
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

        AddSuccess(res.Successes);
        AddError(res.Errors);

        if (res.IsSuccess)
        {
          return RedirectToRoute("view_user", new { Id });
        }
      }
      catch (ValidationException ve)
      {
        AddError(ve);
      }

      return RedirectToRoute("edit_user", new { Id });
    }

    [HttpPost("{Id}/Profile", Name = "edit_user_profile")]
    public virtual async Task<IActionResult> EditProfile(
      [FromRoute] int Id
    , [FromForm] string Name
        ,[FromForm] string CompanyEmail = null        ,[FromForm] string CompanyWebsite = null        ,[FromForm] string CompanyTwitter = null        ,[FromForm] string CompanyDescription = null        )
    {
      try
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

        AddSuccess(res.Successes);
        AddError(res.Errors);

        if (res.IsSuccess)
        {
          return RedirectToRoute("view_user", new { Id });
        }
      }
      catch (ValidationException ve)
      {
        AddError(ve);
      }

      return RedirectToRoute("edit_user", new { Id });
    }

    [HttpPost("{Id}/Roles", Name = "edit_user_roles")]
    public virtual async Task<IActionResult> EditRoles(
        [FromRoute] int Id,
        [FromForm] int[] Roles
    )
    {
      try
      {
        var result = new TaskCompletionSource<Result>();
        var command = new UpdateUserRolesCommand(
            id: Id,
            RoleIds: Roles,

            result: result);

        await _microBus.SendAsync(command);
        var res = await result.Task;

        AddSuccess(res.Successes);
        AddError(res.Errors);

        if (res.IsSuccess)
        {
          return RedirectToRoute("view_user", new { Id });
        }
      }
      catch (ValidationException ve)
      {
        AddError(ve);
      }

      return RedirectToRoute("edit_user", new { Id });
    }

    [HttpPost("{Id}/ChangePassword", Name = "change_user_password")]
    public virtual async Task<IActionResult> ChangePassword(
      [FromRoute] int Id,
            [FromForm] string Password,
            [FromForm] string ConfirmPassword
    )
    {
      try
      {
        var result = new TaskCompletionSource<Result>();
        var command = new SetUserPasswordCommand(
            id: Id,
            Password: Password,
            ConfirmPassword: ConfirmPassword,

            result: result);

        await _microBus.SendAsync(command);
        var res = await result.Task;

        AddSuccess(res.Successes);
        AddError(res.Errors);

        if (res.IsSuccess)
        {
          return RedirectToRoute("view_user", new { Id });
        }
      }
      catch (ValidationException ve)
      {
        AddError(ve);
      }

      return RedirectToRoute("edit_user", new { Id });
    }

    [HttpPost("{Id}/Trash", Name = "trash_user")]
    public virtual async Task<IActionResult> Trash(
        [FromRoute] int Id,
        [FromForm] string ReturnQuery)
    {
      try
      {
        var result = new TaskCompletionSource<Result>();
        var command = new TrashUsersCommand(new int[] { Id }, result);

        await _microBus.SendAsync(command);
        var res = await result.Task;

        AddSuccess(res.Successes);
        AddError(res.Errors);
      }
      catch (ValidationException ve)
      {
        AddError(ve);
      }

      return RedirectToRoute("list_users", ReturnQueryToRouteValues(ReturnQuery));
    }

    [HttpPost("{Id}/Restore", Name = "restore_user")]
    public virtual async Task<IActionResult> Restore([FromRoute] int Id
      , [FromForm] string ReturnQuery)
    {
      try
      {
        var result = new TaskCompletionSource<Result>();
        var command = new RestoreUsersCommand(new int[] { Id }, result);

        await _microBus.SendAsync(command);
        var res = await result.Task;

        AddSuccess(res.Successes);
        AddError(res.Errors);
      }
      catch (ValidationException ve)
      {
        AddError(ve);
      }

      return RedirectToRoute("list_users", ReturnQueryToRouteValues(ReturnQuery));
    }
  }

  #endregion Double-Derived
}
