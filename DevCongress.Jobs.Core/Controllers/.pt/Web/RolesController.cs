using Enexure.MicroBus;
using FluentResults;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using DevCongress.Jobs.Core.Features.Permission.List;
using DevCongress.Jobs.Core.Features.Role.Add;
using DevCongress.Jobs.Core.Features.Role.Fetch;
using DevCongress.Jobs.Core.Features.Role.List;
using DevCongress.Jobs.Core.Features.Role.Restore;
using DevCongress.Jobs.Core.Features.Role.Trash;
using DevCongress.Jobs.Core.Features.Role.Update;
using DevCongress.Jobs.Core.Features.Role.UpdatePermissions;
using DevCongress.Jobs.Core.ViewModels.Roles;
using Plutonium.Reactor.Data.Query.Filter;
using Plutonium.Reactor.Data.Query.Sort;
using System;
using System.Linq;
using System.Threading.Tasks;
using static DevCongress.Jobs.Core.Domain.Model.Role;

namespace DevCongress.Jobs.Core.Controllers.Web
{
  public partial class RolesController : BaseRolesController
  {
    public RolesController(ILogger<RolesController> logger, IMicroBus microBus) : base(logger, microBus)
    {
    }
  }

  #region Double-Derived

  [Route("Roles")]
  public abstract class BaseRolesController : BaseWebController
  {
    protected readonly ILogger<RolesController> _logger;
    protected readonly IMicroBus _microBus;

    protected BaseRolesController(ILogger<RolesController> logger, IMicroBus microBus)
    {
      _logger = logger;
      _microBus = microBus;
    }

    [HttpGet(Name = "list_roles")]
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

      var vm = new ListRolesViewModel()
      {
        Page = Page,
        Limit = Limit,
        SortBy = SortBy,
        SortOrder = SortOrder,
        IncludeTrashed = IncludeTrashed,
      };

      try
      {
        var query = new ListRolesQuery(Page, Limit, filter, CreateDateFilter, UpdateDateFilter, DeleteDateFilter, sort, IncludeTrashed);
        var res = await _microBus.QueryAsync(query);

        if (res.IsSuccess)
        {
          vm = new ListRolesViewModel()
          {
            Roles = res.Roles,

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

    [HttpGet("{Id}", Name = "view_role")]
    public virtual async Task<IActionResult> View([FromRoute] int Id)
    {
      var vm = new ViewRoleViewModel()
      {
      };

      try
      {
        var query = new FetchRoleQuery(Id, true);
        var res = await _microBus.QueryAsync(query);

        if (res.IsSuccess)
        {
          vm = new ViewRoleViewModel()
          {
            Role = res.Role,
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

    [HttpGet("Add", Name = "add_role")]
    public IActionResult Add()
    {
      return View(new AddRoleViewModel());
    }

    [HttpPost("Add", Name = "add_role")]
    public virtual async Task<IActionResult> Add(
        [FromForm] string Name,
        [FromForm] string Description,
        [FromForm] bool IsActive = true
    )
    {
      var vm = new AddRoleViewModel()
      {
      };

      try
      {
        var result = new TaskCompletionSource<Result>();
        var command = new AddRoleCommand(
                    Name: Name,
                    Description: Description,
                    IsActive: IsActive,

          result: result);

        await _microBus.SendAsync(command);
        var res = await result.Task;

        if (res.IsSuccess)
        {
          AddSuccess(res.Successes);
          return RedirectToRoute("list_roles");
        }
        else
        {
          AddError(res.Errors);
          vm = new AddRoleViewModel()
          {
            Name = Name,
            Description = Description,
            IsActive = IsActive,
          };
        }
      }
      catch (ValidationException ve)
      {
        AddError(ve);
      }

      return View(vm);
    }

    [HttpGet("{Id}/Edit", Name = "edit_role")]
    public virtual async Task<IActionResult> Edit([FromRoute] int Id)
    {
      var vm = new EditRoleViewModel()
      {
      };

      try
      {
        var query = new FetchRoleQuery(Id);
        var res = await _microBus.QueryAsync(query);

        if (res.IsSuccess)
        {
          vm = new EditRoleViewModel()
          {
            Role = res.Role,

            IsSuccess = res.IsSuccess,
          };

          var permsQuery = new ListPermissionsQuery(new SortBy<Domain.Model.Permission.PermissionColumn>(Domain.Model.Permission.PermissionColumn.Name, SortDirection.Asc));
          var permsRes = await _microBus.QueryAsync(permsQuery);

          if (permsRes.IsSuccess)
          {
            vm.Permissions = permsRes.Permissions.ToArray();
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

    [HttpPost("{Id}/Edit", Name = "edit_role")]
    public virtual async Task<IActionResult> Edit(
        [FromRoute] int Id,
        [FromForm] string Name,
        [FromForm] string Description,
        [FromForm] bool IsActive = true
    )
    {
      try
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

        AddSuccess(res.Successes);
        AddError(res.Errors);

        if (res.IsSuccess)
        {
          return RedirectToRoute("view_role", new { Id });
        }
      }
      catch (ValidationException ve)
      {
        AddError(ve);
      }

      return RedirectToRoute("edit_role", new { Id });
    }

    [HttpPost("{Id}/Permissions", Name = "edit_role_permissions")]
    public virtual async Task<IActionResult> EditPermissions(
        [FromRoute] int Id,
        [FromForm] int[] Permissions
    )
    {
      try
      {
        var result = new TaskCompletionSource<Result>();
        var command = new UpdateRolePermissionsCommand(
            id: Id,
            Permissions: Permissions,

            result: result);

        await _microBus.SendAsync(command);
        var res = await result.Task;

        AddSuccess(res.Successes);
        AddError(res.Errors);

        if (res.IsSuccess)
        {
          return RedirectToRoute("view_role", new { Id });
        }
      }
      catch (ValidationException ve)
      {
        AddError(ve);
      }

      return RedirectToRoute("edit_role", new { Id });
    }

    [HttpPost("{Id}/Trash", Name = "trash_role")]
    public virtual async Task<IActionResult> Trash([FromRoute] int Id)
    {
      try
      {
        var result = new TaskCompletionSource<Result>();
        var command = new TrashRolesCommand(new int[] { Id }, result);

        await _microBus.SendAsync(command);
        var res = await result.Task;

        AddSuccess(res.Successes);
        AddError(res.Errors);
      }
      catch (ValidationException ve)
      {
        AddError(ve);
      }

      return RedirectToRoute("list_roles");
    }

    [HttpPost("{Id}/Restore", Name = "restore_role")]
    public virtual async Task<IActionResult> Restore([FromRoute] int Id)
    {
      try
      {
        var result = new TaskCompletionSource<Result>();
        var command = new RestoreRolesCommand(new int[] { Id }, result);

        await _microBus.SendAsync(command);
        var res = await result.Task;

        AddSuccess(res.Successes);
        AddError(res.Errors);
      }
      catch (ValidationException ve)
      {
        AddError(ve);
      }

      return RedirectToRoute("list_roles");
    }
  }

  #endregion Double-Derived
}
