using Dapper;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Npgsql;
using DevCongress.Jobs.Core.Domain.Model;
using Plutonium.Reactor.Data;
using Plutonium.Reactor.Data.Query.Filter;
using Plutonium.Reactor.Data.Query.Sort;
using Plutonium.Reactor.Options;
using Plutonium.Reactor.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static DevCongress.Jobs.Core.Domain.Model.Role;

namespace DevCongress.Jobs.Core.Domain.Repository
{
  internal partial interface IRoleRepository
  {
    Task<int> Count(
      RoleFilter filter = RoleFilter.All
      , DateFilter createDateFilter = default(DateFilter)
      , DateFilter updateDateFilter = default(DateFilter)
      , DateFilter deleteDateFilter = default(DateFilter)
      , bool includeTrashed = false);

    Task<IEnumerable<Role>> List(
      int offset = 0
      , int limit = 10
      , RoleFilter filter = RoleFilter.All
      , DateFilter createDateFilter = default(DateFilter)
      , DateFilter updateDateFilter = default(DateFilter)
      , DateFilter deleteDateFilter = default(DateFilter)
      , SortBy<RoleColumn> sortBy = default(SortBy<RoleColumn>)
      , bool includeTrashed = false);

    Task<DetailedRole> Fetch(int id);

    Task<int> Add(
            string Name,
            string Description,
            bool IsActive = true,

      int modifiedBy = 0);

    Task<int> Update(
      int id,
            string Name,
            string Description,
            bool IsActive = true,

      int modifiedBy = 0);

    Task<int> Trash(int[] ids, int modifiedBy);

    Task<int> Restore(int[] ids, int modifiedBy);

    Task<IEnumerable<RoleAuditLog>> GetLogs(int id, int Limit = 0);

    Task<IEnumerable<Permission>> GetPermissions(int id);

    Task<int> UpdatePermissions(
      int id,
        int[] PermissionIds,
      int modifiedBy = 0);
  }

  internal partial class RoleRepository : BaseRoleRepository, IRoleRepository
  {
    public RoleRepository(IOptions<ConnectionString> options, IConnectionResolver<NpgsqlConnection> resolver) : base(options, resolver)
    {
    }
  }

  #region Double-Derived

  internal abstract class BaseRoleRepository : BasePgRepository
  {
    protected BaseRoleRepository(IOptions<ConnectionString> options, IConnectionResolver<NpgsqlConnection> resolver) : base(options, resolver)
    {
    }

    public virtual Task<int> Count(
      RoleFilter filter = RoleFilter.All
      , DateFilter createDateFilter = default(DateFilter)
      , DateFilter updateDateFilter = default(DateFilter)
      , DateFilter deleteDateFilter = default(DateFilter)
      , bool includeTrashed = false)
    {
      return WithConnection(conn =>
      {
        var query = @"SELECT
	                        count(1)
                            FROM public.""role"" a
                        ";

        query += FilterClause(filter, createDateFilter, updateDateFilter, deleteDateFilter, includeTrashed);

        return conn.ExecuteScalarAsync<int>(query, new
        {
          CreateStartDate = createDateFilter?.Start,
          CreateEndDate = createDateFilter?.End,
          UpdateStartDate = updateDateFilter?.End,
          UpdateEndDate = updateDateFilter?.End,
          DeleteStartDate = deleteDateFilter?.End,
          DeleteEndDate = deleteDateFilter?.End,
        });
      });
    }

    public virtual Task<IEnumerable<Role>> List(
      int offset = 0
      , int limit = 10
      , RoleFilter filter = RoleFilter.All
      , DateFilter createDateFilter = default(DateFilter)
      , DateFilter updateDateFilter = default(DateFilter)
      , DateFilter deleteDateFilter = default(DateFilter)
      , SortBy<RoleColumn> sortBy = default(SortBy<RoleColumn>)
      , bool includeTrashed = false)
    {
      return WithConnection(conn =>
      {
        var query = @"SELECT
	                        a.*
                            FROM public.""role"" a
                        ";

        query += FilterClause(filter, createDateFilter, updateDateFilter, deleteDateFilter, includeTrashed);

        query += $@"ORDER BY {SortClause(sortBy)}
                    OFFSET @Offset
                  ";

        if (limit > 0)
        {
          query += @"LIMIT @Limit
                         ";
        }

        return conn.QueryAsync<Role>(query, new
        {
          CreateStartDate = createDateFilter?.Start,
          CreateEndDate = createDateFilter?.End,
          UpdateStartDate = updateDateFilter?.End,
          UpdateEndDate = updateDateFilter?.End,
          DeleteStartDate = deleteDateFilter?.End,
          DeleteEndDate = deleteDateFilter?.End,

          Offset = offset,
          Limit = limit,
        });
      });
    }

    public virtual Task<DetailedRole> Fetch(int id)
    {
      return WithConnection(conn =>
      {
        var query = @"SELECT
	                              a.*
                                  FROM public.""role"" a
                                  WHERE a.id = @Id
                              ";

        return conn.QuerySingleOrDefaultAsync<DetailedRole>(query, new
        {
          Id = id,
        });
      });
    }

    public virtual Task<int> Add(
        string Name,
        string Description,
        bool IsActive = true,

        int modifiedBy = 0)
    {
      return WithConnection(conn =>
      {
        const string query = @"
                            WITH new_role AS (
                                INSERT INTO public.""role""
                                (
                                  name,
                                  description,
                                  is_active,

                                  created_by,
                                  updated_by
                                )
                                VALUES
                                (
                                  @Name,
                                  @Description,
                                  @IsActive,

                                  @ModifiedBy,
                                  @ModifiedBy
                                )
                                RETURNING *
                            )
                            INSERT INTO public.role_audit_log
                            (user_id, action_name, description, object_name, object_data, target_role_id)
                            SELECT
                            created_by, 'role.add', 'added role', 'role', row_to_json(new_role), id
                            FROM new_role
                            RETURNING target_role_id;
                        ";

        return conn.ExecuteScalarAsync<int>(query, new
        {
          Name,
          Description,
          IsActive,

          ModifiedBy = modifiedBy,
        });
      });
    }

    public virtual Task<int> Update(int id,
        string Name,
        string Description,
        bool IsActive = true,

        int modifiedBy = 0)
    {
      return WithConnection(conn =>
      {
        const string query = @"
                            WITH updated_role AS (
	                            UPDATE public.""role""
                                SET
                                  name = @Name,
                                  description = @Description,
                                  is_active = @IsActive,

                                  updated_at = now(),
                                  updated_by = @ModifiedBy
	                            WHERE id = @Id
	                            RETURNING *
                            )
                            INSERT INTO public.role_audit_log
                            (user_id, action_name, description, object_name, object_data, target_role_id)
                            SELECT
                            @ModifiedBy, 'role.update', 'updated role', 'role', row_to_json(updated_role), id
                            FROM updated_role;
                        ";

        return conn.ExecuteAsync(query, new
        {
          Id = id,
          Name,
          Description,
          IsActive,

          ModifiedBy = modifiedBy,
        });
      });
    }

    public virtual Task<int> Trash(int[] ids, int modifiedBy)
    {
      return WithConnection(conn =>
      {
        const string query = @"
                            WITH deleted_role AS (
	                            UPDATE public.""role""
                                SET
                                    deleted_at = now()
                                ,   deleted_by = @ModifiedBy
                                ,   updated_at = now()
                                ,   updated_by = @ModifiedBy
	                            WHERE id = @Id
                                AND deleted_at IS NULL
	                            RETURNING *
                            )
                            INSERT INTO public.role_audit_log
                            (user_id, action_name, description, object_name, object_data, target_role_id)
                            SELECT
                            @ModifiedBy, 'role.trash', 'trashed role', 'role', row_to_json(deleted_role), id
                            FROM deleted_role;
                        ";

        return conn.ExecuteAsync(query, ids.Select(id => new
        {
          Id = id,
          ModifiedBy = modifiedBy,
        }));
      });
    }

    public virtual Task<int> Restore(int[] ids, int modifiedBy)
    {
      return WithConnection(conn =>
      {
        const string query = @"
                            WITH restored_role AS (
	                            UPDATE public.""role""
                                SET
                                    deleted_at = NULL
                                ,   deleted_by = NULL
                                ,   updated_at = now()
                                ,   updated_by = @ModifiedBy
	                            WHERE id = @Id
                                AND deleted_at IS NOT NULL
	                            RETURNING *
                            )
                            INSERT INTO public.role_audit_log
                            (user_id, action_name, description, object_name, object_data, target_role_id)
                            SELECT
                            @ModifiedBy, 'role.restore', 'restored role', 'role', row_to_json(restored_role), id
                            FROM restored_role;
                        ";

        return conn.ExecuteAsync(query, ids.Select(id => new
        {
          Id = id,
          ModifiedBy = modifiedBy,
        }));
      });
    }

    public virtual Task<IEnumerable<RoleAuditLog>> GetLogs(int id, int limit = 0)
    {
      return WithConnection(conn =>
      {
        var query = @"SELECT
	                          l.*
                          , u.username
                          FROM public.role_audit_log l
                          JOIN public.""user"" u ON user_id = u.id
                          WHERE target_role_id = @Id
                          ORDER BY l.id desc
                      ";

        if (limit > 0)
        {
          query += @"LIMIT @Limit
                         ";
        }

        return conn.QueryAsync<RoleAuditLog>(query, new
        {
          Id = id,
          Limit = limit,
        });
      });
    }

    public virtual Task<IEnumerable<Permission>> GetPermissions(int id)
    {
      return WithConnection(conn =>
      {
        const string query = @"SELECT
	                              p.*
                                  FROM public.""permission"" p
                                  JOIN public.""permission_role"" pr ON pr.role_id = @RoleId
                                  AND pr.permission_id = p.id
                                  ORDER BY p.name
                              ";

        return conn.QueryAsync<Permission>(query, new
        {
          RoleId = id
        });
      });
    }

    public virtual Task<int> UpdatePermissions(int id,
        int[] PermissionIds,

        int modifiedBy = 0)
    {
      return WithConnection(async conn =>
      {
        const string removePermissionsQuery = @"
	                            DELETE FROM public.""permission_role"" pr
                                WHERE pr.role_id = @RoleId
                                AND pr.permission_id <> ANY(@PermissionIds)
                        ";

        const string addPermissionQuery = @"
                                WITH new_permission_ids AS (
                                    SELECT p.id permission_id FROM public.""permission"" p
                                    WHERE p.id NOT IN (
                                        SELECT pr.permission_id from public.""permission_role"" pr
                                        WHERE pr.role_id = @RoleId
                                    )
                                    AND p.id = ANY(@PermissionIds)
                                )
                                INSERT INTO public.""permission_role""
                                    (permission_id, role_id, created_by)
                                SELECT
                                    new_permission_ids.permission_id, @RoleId, @ModifiedBy
                                FROM new_permission_ids;
                        ";

        const string auditQuery = @"
                            INSERT INTO public.role_audit_log
                            (user_id, action_name, description, object_name, object_data, target_role_id)
                            SELECT
                            @ModifiedBy, 'role.update_permissions', 'updated role permissions', 'role', @PermissionStr::jsonb, @RoleId;
                        ";

        await conn.ExecuteAsync(removePermissionsQuery, new
        {
          RoleId = id,
          PermissionIds = PermissionIds
        }).ConfigureAwait(false);

        await conn.ExecuteAsync(addPermissionQuery, new
        {
          RoleId = id,
          PermissionIds = PermissionIds,

          ModifiedBy = modifiedBy,
        }).ConfigureAwait(false);

        return await conn.ExecuteAsync(auditQuery, new
        {
          RoleId = id,
          PermissionStr = JsonConvert.SerializeObject(PermissionIds),

          ModifiedBy = modifiedBy,
        }).ConfigureAwait(false);
      });
    }

    #region helpers

    private string FilterClause(
      RoleFilter filter = RoleFilter.All
      , DateFilter createDateFilter = default(DateFilter)
      , DateFilter updateDateFilter = default(DateFilter)
      , DateFilter deleteDateFilter = default(DateFilter)
      , bool includeTrashed = false)
    {
      var filtered = true;
      var clause = " ";

      #region filter

      switch (filter)
      {
        case RoleFilter.All:
          filtered = false;
          break;

        case RoleFilter.Trashed:
          clause += $"WHERE a.deleted_at IS NOT NULL ";
          break;

        default:
          throw new NotImplementedException($"Unknown role filter: '{Convert.ToString(filter)}'");
      }

      if (!includeTrashed && filter != RoleFilter.Trashed)
      {
        clause += $" {(filtered ? "AND" : "WHERE")} a.deleted_at IS NULL ";
        filtered = true;
      }

      #endregion filter

      #region dateFilters

      if (createDateFilter?.Start != null)
      {
        clause += $" {(filtered ? "AND" : "WHERE")} a.created_at >= @CreateStartDate ";
        filtered = true;
      }

      if (createDateFilter?.End != null)
      {
        var cmp = createDateFilter?.IncludeEndDate ?? false ? "<=" : "<";
        clause += $" {(filtered ? "AND" : "WHERE")} a.created_at {cmp} @CreateEndDate ";
        filtered = true;
      }

      if (updateDateFilter?.Start != null)
      {
        clause += $" {(filtered ? "AND" : "WHERE")} a.updated_at >= @UpdateStartDate ";
        filtered = true;
      }

      if (updateDateFilter?.End != null)
      {
        var cmp = updateDateFilter?.IncludeEndDate ?? false ? "<=" : "<";
        clause += $" {(filtered ? "AND" : "WHERE")} a.updated_at {cmp} @UpdateEndDate ";
        filtered = true;
      }

      if (deleteDateFilter?.Start != null)
      {
        clause += $" {(filtered ? "AND" : "WHERE")} a.deleted_at >= @DeleteStartDate ";
        filtered = true;
      }

      if (deleteDateFilter?.End != null)
      {
        var cmp = deleteDateFilter?.IncludeEndDate ?? false ? "<=" : "<";
        clause += $" {(filtered ? "AND" : "WHERE")} a.deleted_at {cmp} @DeleteEndDate ";
        filtered = true;
      }

      #endregion dateFilters

      return clause;
    }

    private string SortClause(SortBy<RoleColumn> sortBy)
    {
      string sortColum;
      switch (sortBy?.Column ?? RoleColumn.Id)
      {
        case RoleColumn.Id:
          sortColum = "id";
          break;

        case RoleColumn.Name:
          sortColum = "name";
          break;

        case RoleColumn.Description:
          sortColum = "description";
          break;

        case RoleColumn.IsActive:
          sortColum = "is_active";
          break;

        case RoleColumn.CreatedBy:
          sortColum = "created_by";
          break;

        case RoleColumn.CreatedAt:
          sortColum = "created_at";
          break;

        case RoleColumn.UpdatedBy:
          sortColum = "updated_by";
          break;

        case RoleColumn.UpdatedAt:
          sortColum = "updated_at";
          break;

        case RoleColumn.DeletedBy:
          sortColum = "deleted_by";
          break;

        case RoleColumn.DeletedAt:
          sortColum = "deleted_at";
          break;

        default:
          throw new NotImplementedException($"Unknown role sort column: '{Convert.ToString(sortBy.Column)}'");
      }

      string sortOrder;
      switch (sortBy?.Direction ?? SortDirection.Desc)
      {
        case SortDirection.Asc:
          sortOrder = "asc";
          break;

        default:
          sortOrder = "desc";
          break;
      }

      return $"a.{sortColum} {sortOrder}";
    }

    #endregion helpers
  }

  #endregion Double-Derived
}
