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
using static DevCongress.Jobs.Core.Domain.Model.User;


namespace DevCongress.Jobs.Core.Domain.Repository
{
  internal partial interface IUserRepository
  {
    Task<int> Count(
      int tenantId
      , UserFilter filter = UserFilter.All
      , DateFilter createDateFilter = default(DateFilter)
      , DateFilter updateDateFilter = default(DateFilter)
      , DateFilter deleteDateFilter = default(DateFilter)
      , bool includeTrashed = false);

    Task<IEnumerable<User>> List(
      int tenantId
      , int offset = 0
      , int limit = 10
      , UserFilter filter = UserFilter.All
      , DateFilter createDateFilter = default(DateFilter)
      , DateFilter updateDateFilter = default(DateFilter)
      , DateFilter deleteDateFilter = default(DateFilter)
      , SortBy<UserColumn> sortBy = default(SortBy<UserColumn>)
      , bool includeTrashed = false);

    Task<DetailedUser> Fetch(int id, int tenantId);

    Task<DetailedUser> FindByUsername(string username);

    Task<int> Add(
            int? TenantId,
            string Username,
            string Password,
            DateTimeOffset? LastLogin = null,
            DateTimeOffset? LockoutEnd = null,
            DateTimeOffset? BanEnd = null,
            bool IsConfirmed = false,
            bool IsActive = true,

            int modifiedBy = 0);

    Task<int> Update(
        int id,
        int tenantId,
        string Username,
        DateTimeOffset? LastLogin = null,
        DateTimeOffset? LockoutEnd = null,
        DateTimeOffset? BanEnd = null,
        bool IsConfirmed = false,
        bool IsActive = true,
        int modifiedBy = 0);

    Task<int> ChangePassword(
        int id,
        int tenantId,
        string Password,
        int modifiedBy = 0);

    Task<int> Trash(
        int[] ids,
        int tenantId,
        int modifiedBy);

    Task<int> Restore(
        int[] ids,
        int tenantId,
        int modifiedBy);

    Task<IEnumerable<UserAuditLog>> GetLogs(
        int id,
        int limit = 0,
        int logsBy = 0);

    Task<IEnumerable<Role>> GetRoles(int id);

    Task<IEnumerable<Permission>> GetPermissions(int id);

    Task<int> UpdateRoles(
      int id,
      int[] RoleIds,
      int modifiedBy = 0);

    Task<int> Login(int id, string ipAddress);

    Task<int> LoginFailed(int id, string ipAddress);

    Task<DetailedUserProfile> FetchProfile(int UserId);

    Task<int> AddProfile(
            int UserId,
            string Name,
                        string CompanyName = "N/A",
                        string CompanyEmail = null,
                        string CompanyWebsite = null,
                        string CompanyDescription = null,
            
            int modifiedBy = 0);

    Task<int> UpdateProfile(
            int UserId,
            int tenantId,
            string Name,
                        string CompanyName = "N/A",
                        string CompanyEmail = null,
                        string CompanyWebsite = null,
                        string CompanyDescription = null,
            
            int modifiedBy = 0);
  }

  internal partial class UserRepository : BaseUserRepository, IUserRepository
  {
    public UserRepository(IOptions<ConnectionString> options, IConnectionResolver<NpgsqlConnection> resolver) : base(options, resolver)
    {
    }
  }

  #region Double-Derived

  internal abstract class BaseUserRepository : BasePgRepository
  {
    protected BaseUserRepository(IOptions<ConnectionString> options, IConnectionResolver<NpgsqlConnection> resolver) : base(options, resolver)
    {
    }

    public virtual Task<int> Count(
      int tenantId
      , UserFilter filter = UserFilter.All
      , DateFilter createDateFilter = default(DateFilter)
      , DateFilter updateDateFilter = default(DateFilter)
      , DateFilter deleteDateFilter = default(DateFilter)
      , bool includeTrashed = false)
    {
      return WithConnection(conn =>
      {
        var query = @"SELECT
	                        count(1)
                            FROM public.""user"" a
                        ";

        query += FilterClause(tenantId, filter, createDateFilter, updateDateFilter, deleteDateFilter, includeTrashed);

        return conn.ExecuteScalarAsync<int>(query, new
        {
          TenantId = tenantId,
          CreateStartDate = createDateFilter?.Start,
          CreateEndDate = createDateFilter?.End,
          UpdateStartDate = updateDateFilter?.End,
          UpdateEndDate = updateDateFilter?.End,
          DeleteStartDate = deleteDateFilter?.End,
          DeleteEndDate = deleteDateFilter?.End,
        });
      });
    }

    public virtual Task<IEnumerable<User>> List(
      int tenantId
      , int offset = 0
      , int limit = 10
      , UserFilter filter = UserFilter.All
      , DateFilter createDateFilter = default(DateFilter)
      , DateFilter updateDateFilter = default(DateFilter)
      , DateFilter deleteDateFilter = default(DateFilter)
      , SortBy<UserColumn> sortBy = default(SortBy<UserColumn>)
      , bool includeTrashed = false)
    {
      return WithConnection(conn =>
      {
        var query = @"SELECT
	                        a.*
                            FROM public.""user"" a
                        ";

        query += FilterClause(tenantId, filter, createDateFilter, updateDateFilter, deleteDateFilter, includeTrashed);

        query += $@"ORDER BY {SortClause(sortBy)}
                    OFFSET @Offset
                  ";

        if (limit > 0)
        {
          query += @"LIMIT @Limit
                         ";
        }

        return conn.QueryAsync<User>(query, new
        {
          TenantId = tenantId,
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

    public virtual Task<DetailedUser> Fetch(
        int id,
        int tenantId)
    {
      return WithConnection(conn =>
      {
        var query = @"SELECT
	                              a.*
                                  FROM public.""user"" a
                                  WHERE a.id = @Id
                              ";

        if (tenantId > 0)
        {
          query += @"AND a.tenant_id = @TenantId
                  ";
        }

        return conn.QuerySingleOrDefaultAsync<DetailedUser>(query, new
        {
          Id = id,
          TenantId = tenantId,
        });
      });
    }

    public virtual Task<DetailedUser> FindByUsername(string username)
    {
      return WithConnection(conn =>
      {
        const string query = @"SELECT
	                              a.*
                                  FROM public.""user"" a
                                  WHERE a.username = @Username
                              ";

        return conn.QuerySingleOrDefaultAsync<DetailedUser>(query, new
        {
          Username = username.ToLowerInvariant(),
        });
      });
    }

    public virtual Task<int> Add(
            int? TenantId,
            string Username,
            string Password,
            DateTimeOffset? LastLogin = null,
            DateTimeOffset? LockoutEnd = null,
            DateTimeOffset? BanEnd = null,
            bool IsConfirmed = false,
            bool IsActive = true,
            int modifiedBy = 0)
    {
      return WithConnection(conn =>
      {
        var tenantValue = TenantId is null ? "currval('public.user_id_seq')" : "@TenantId";
        var query = $@"
                            WITH new_user AS (
                                INSERT INTO public.""user""
                                (
                                  tenant_id,
                                  username,
                                  password,
                                  last_login,
                                  lockout_end,
                                  ban_end,
                                  is_confirmed,
                                  is_active,

                                  created_by,
                                  updated_by
                                )
                                VALUES
                                (
                                  {tenantValue},
                                  @Username,
                                  @Password,
                                  @LastLogin,
                                  @LockoutEnd,
                                  @BanEnd,
                                  @IsConfirmed,
                                  @IsActive,

                                  @ModifiedBy,
                                  @ModifiedBy
                                )
                                RETURNING *
                            )
                            INSERT INTO public.user_audit_log
                            (user_id, action_name, description, object_name, object_data, target_user_id)
                            SELECT
                            created_by, 'user.add', 'added user', 'user', row_to_json(new_user), id
                            FROM new_user
                            RETURNING target_user_id;
                        ";

        return conn.ExecuteScalarAsync<int>(query, new
        {
          TenantId,
          Username = Username.ToLowerInvariant(),
          Password = Password is null ? "" : BCrypt.Net.BCrypt.HashPassword(Password, BCrypt.Net.SaltRevision.Revision2Y),
          LastLogin,
          LockoutEnd,
          BanEnd,
          IsConfirmed,
          IsActive,

          ModifiedBy = modifiedBy,
        });
      });
    }

    public virtual Task<int> Update(
            int id,
            int tenantId,
            string Username,
            DateTimeOffset? LastLogin = null,
            DateTimeOffset? LockoutEnd = null,
            DateTimeOffset? BanEnd = null,
            bool IsConfirmed = false,
            bool IsActive = true,
            int modifiedBy = 0)
    {
      return WithConnection(conn =>
      {
        var tenantClause = "";
        if (tenantId > 0)
        {
          tenantClause = "AND tenant_id = @TenantId";
        }

        var query = $@"
                            WITH updated_user AS (
	                            UPDATE public.""user""
                                SET
                                  username = @Username,
                                  last_login = @LastLogin,
                                  lockout_end = @LockoutEnd,
                                  ban_end = @BanEnd,
                                  is_confirmed = @IsConfirmed,
                                  is_active = @IsActive,

                                  updated_at = now(),
                                  updated_by = @ModifiedBy
	                            WHERE id = @Id
                                {tenantClause}
	                            RETURNING *
                            )
                            INSERT INTO public.user_audit_log
                            (user_id, action_name, description, object_name, object_data, target_user_id)
                            SELECT
                            @ModifiedBy, 'user.update', 'updated user', 'user', row_to_json(updated_user), id
                            FROM updated_user;
                        ";

        return conn.ExecuteAsync(query, new
        {
          Id = id,
          TenantId = tenantId,
          Username = Username.ToLowerInvariant(),
          LastLogin,
          LockoutEnd,
          BanEnd,
          IsConfirmed,
          IsActive,

          ModifiedBy = modifiedBy,
        });
      });
    }

    public virtual Task<int> ChangePassword(
            int id,
            int tenantId,
            string Password,
            int modifiedBy = 0)
    {
      return WithConnection(conn =>
      {
        var tenantClause = "";
        if (tenantId > 0)
        {
          tenantClause = "AND tenant_id = @TenantId";
        }

        var query = $@"
                            WITH updated_user AS (
	                            UPDATE public.""user""
                                SET
                                  password = @Password,

                                  updated_at = now(),
                                  updated_by = @ModifiedBy
	                            WHERE id = @Id
                                {tenantClause}
	                            RETURNING *
                            )
                            INSERT INTO public.user_audit_log
                            (user_id, action_name, description, object_name, object_data, target_user_id)
                            SELECT
                            @ModifiedBy, 'user.update', 'changed user password', 'user', row_to_json(updated_user), id
                            FROM updated_user;
                        ";

        return conn.ExecuteAsync(query, new
        {
          Id = id,
          Password = BCrypt.Net.BCrypt.HashPassword(Password, BCrypt.Net.SaltRevision.Revision2Y),

          TenantId = tenantId,
          ModifiedBy = modifiedBy,
        });
      });
    }

    public virtual Task<int> Trash(
        int[] ids,
        int tenantId,
        int modifiedBy)
    {
      return WithConnection(conn =>
      {
        var tenantClause = "";
        if (tenantId > 0)
        {
          tenantClause = "AND tenant_id = @TenantId";
        }

        var query = $@"
                            WITH deleted_user AS (
	                            UPDATE public.""user""
                                SET
                                    deleted_at = now()
                                ,   deleted_by = @ModifiedBy
                                ,   updated_at = now()
                                ,   updated_by = @ModifiedBy
	                            WHERE id = @Id
                                AND deleted_at IS NULL
                                {tenantClause}
	                            RETURNING *
                            )
                            INSERT INTO public.user_audit_log
                            (user_id, action_name, description, object_name, object_data, target_user_id)
                            SELECT
                            @ModifiedBy, 'user.trash', 'trashed user', 'user', row_to_json(deleted_user), id
                            FROM deleted_user;
                        ";

        return conn.ExecuteAsync(query, ids.Select(id => new
        {
          Id = id,
          TenantId = tenantId,
          ModifiedBy = modifiedBy,
        }));
      });
    }

    public virtual Task<int> Restore(
        int[] ids,
        int tenantId,
        int modifiedBy)
    {
      return WithConnection(conn =>
      {
        var tenantClause = "";
        if (tenantId > 0)
        {
          tenantClause = "AND tenant_id = @TenantId";
        }

        var query = $@"
                            WITH restored_user AS (
	                            UPDATE public.""user""
                                SET
                                    deleted_at = NULL
                                ,   deleted_by = NULL
                                ,   updated_at = now()
                                ,   updated_by = @ModifiedBy
	                            WHERE id = @Id
                                AND deleted_at IS NOT NULL
                                {tenantClause}
	                            RETURNING *
                            )
                            INSERT INTO public.user_audit_log
                            (user_id, action_name, description, object_name, object_data, target_user_id)
                            SELECT
                            @ModifiedBy, 'user.restore', 'restored user', 'user', row_to_json(restored_user), id
                            FROM restored_user;
                        ";

        return conn.ExecuteAsync(query, ids.Select(id => new
        {
          Id = id,
          TenantId = tenantId,
          ModifiedBy = modifiedBy,
        }));
      });
    }

    public virtual Task<IEnumerable<UserAuditLog>> GetLogs(
        int id,
        int limit = 0,
        int logsBy = 0)
    {
      return WithConnection(conn =>
      {
        var byQuery = "";
        if (logsBy > 0)
        {
          byQuery = @" AND user_id = @LogsBy
                         ";
        }

        var query = $@"SELECT
	                          l.*
                          , u.username
                          FROM public.user_audit_log l
                          JOIN public.""user"" u ON user_id = u.id
                          WHERE target_user_id = @Id
                            {byQuery}
                          ORDER BY l.id desc
                      ";

        if (limit > 0)
        {
          query += @"LIMIT @Limit
                         ";
        }

        return conn.QueryAsync<UserAuditLog>(query, new
        {
          Id = id,
          Limit = limit,
          LogsBy = logsBy,
        });
      });
    }

    public virtual Task<IEnumerable<Role>> GetRoles(int id)
    {
      return WithConnection(conn =>
      {
        const string query = @"SELECT
	                              r.*
                                  FROM public.""role"" r
                                  JOIN public.""role_user"" ru ON ru.user_id = @UserId
                                  AND ru.role_id = r.id
                                  WHERE r.is_active = true
                                  ORDER BY r.name
                              ";

        return conn.QueryAsync<Role>(query, new
        {
          UserId = id
        });
      });
    }

    public virtual Task<IEnumerable<Permission>> GetPermissions(int id)
    {
      return WithConnection(conn =>
      {
        const string query = @"SELECT
	                              DISTINCT p.*
                                  FROM public.""permission"" p
                                  JOIN public.""permission_role"" pr ON pr.permission_id = p.id
                                  WHERE pr.role_id IN (SELECT ru.role_id FROM public.""role_user"" ru WHERE ru.user_id = @UserId)
                                  ORDER BY p.name
                              ";

        return conn.QueryAsync<Permission>(query, new
        {
          UserId = id
        });
      });
    }

    public virtual Task<int> UpdateRoles(
        int id,
        int[] RoleIds,
        int modifiedBy = 0)
    {
      return WithConnection(async conn =>
      {
        const string removeRolesQuery = @"
	                            DELETE FROM public.""role_user"" ru
                                WHERE ru.user_id = @UserId
                                AND ru.role_id <> ANY(@RoleIds)
                        ";

        const string addRolesQuery = @"
                                WITH new_role_ids AS (
                                    SELECT r.id role_id FROM public.""role"" r
                                    WHERE r.id NOT IN (
                                        SELECT ru.role_id from public.""role_user"" ru
                                        WHERE ru.user_id = @UserId
                                    )
                                    AND r.id = ANY(@RoleIds)
                                )
                                INSERT INTO public.""role_user""
                                    (role_id, user_id, created_by)
                                SELECT
                                    new_role_ids.role_id, @UserId, @ModifiedBy
                                FROM new_role_ids;
                        ";

        const string auditQuery = @"
                            INSERT INTO public.user_audit_log
                            (user_id, action_name, description, object_name, object_data, target_user_id)
                            SELECT
                            @ModifiedBy, 'user.update_permissions', 'updated user roles', 'user', @RoleStr::jsonb, @UserId;
                        ";

        await conn.ExecuteAsync(removeRolesQuery, new
        {
          UserId = id,
          RoleIds = RoleIds
        }).ConfigureAwait(false);

        await conn.ExecuteAsync(addRolesQuery, new
        {
          UserId = id,
          RoleIds = RoleIds,

          ModifiedBy = modifiedBy,
        }).ConfigureAwait(false);

        return await conn.ExecuteAsync(auditQuery, new
        {
          UserId = id,
          RoleStr = JsonConvert.SerializeObject(RoleIds),

          ModifiedBy = modifiedBy,
        }).ConfigureAwait(false);
      });
    }

    public virtual Task<int> Login(
        int id,
        string ipAddress)
    {
      return WithConnection(conn =>
      {
        const string query = @"
                            WITH logged_in_user AS (
	                            UPDATE public.""user""
                                SET
                                  last_login = now(),
                                  lockout_end = NULL,

                                  updated_at = now(),
                                  updated_by = @ModifiedBy
	                            WHERE id = @Id
	                            RETURNING *
                            )
                            INSERT INTO public.""user_audit_log""
                            (user_id, action_name, description, object_name, object_data, target_user_id)
                            SELECT
                            @ModifiedBy, 'user.login.success', 'logged in: ' || @IpAddress, 'user', row_to_json(logged_in_user), id
                            FROM logged_in_user;
                        ";

        return conn.ExecuteAsync(query, new
        {
          id,
          IpAddress = ipAddress,
          ModifiedBy = id,
        });
      });
    }

    public virtual Task<int> LoginFailed(
        int id,
        string ipAddress)
    {
      return WithConnection(conn =>
      {
        const string query = @"
                            WITH failed_login_user AS (
	                            SELECT * FROM public.""user""
	                            WHERE id = @Id
                            )
                            INSERT INTO public.""user_audit_log""
                            (user_id, action_name, description, object_name, object_data, target_user_id)
                            SELECT
                            @ModifiedBy, 'user.login.fail', 'failed login: ' || @IpAddress, 'user', row_to_json(failed_login_user), id
                            FROM failed_login_user;

                            WITH locked_out_user AS (
	                            UPDATE public.""user""
                                SET
                                  lockout_end = @LockoutEnd,

                                  updated_at = now(),
                                  updated_by = @ModifiedBy
	                            WHERE id = @Id
                              AND (SELECT count(1) FROM public.""user_audit_log"" ul WHERE ul.target_user_id = @Id AND ul.action_name = 'user.login.fail' AND ul.created_at >=  @FailedLoginCheck) >= 5
                              RETURNING *
                            )
                            INSERT INTO public.""user_audit_log""
                            (user_id, action_name, description, object_name, object_data, target_user_id)
                            SELECT
                            @ModifiedBy, 'user.login.locked_out', 'locked out', 'user', row_to_json(locked_out_user), id
                            FROM locked_out_user;
                        ";

        return conn.ExecuteAsync(query, new
        {
          Id = id,
          IpAddress = ipAddress,
          ModifiedBy = id,
          LockoutEnd = DateTimeOffset.Now.AddMinutes(5),
          FailedLoginCheck = DateTimeOffset.Now.AddMinutes(-5),
        });
      });
    }

    #region Profile

    public virtual Task<DetailedUserProfile> FetchProfile(int UserId)
    {
      return WithConnection(conn =>
      {
        const string query = @"SELECT
	                              a.*
                                  FROM public.""user_profile"" a
                                  WHERE a.user_id = @UserId
                              ";

        return conn.QuerySingleOrDefaultAsync<DetailedUserProfile>(query, new
        {
          UserId
        });
      });
    }

    public virtual Task<int> AddProfile(
            int UserId,
            string Name,
                        string CompanyName = "N/A",
                        string CompanyEmail = null,
                        string CompanyWebsite = null,
                        string CompanyDescription = null,
            
            int modifiedBy = 0)
    {
      return WithConnection(conn =>
      {
        const string query = @"
                            WITH new_user_profile AS (
                                INSERT INTO public.""user_profile""
                                (
                                  user_id,
                                  name,

                                  company_name,
                                  company_email,
                                  company_website,
                                  company_description,


                                  created_by,
                                  updated_by
                                )
                                VALUES
                                (
                                  @UserId,
                                  @Name,

                                  @CompanyName,
                                  @CompanyEmail,
                                  @CompanyWebsite,
                                  @CompanyDescription,


                                  @ModifiedBy,
                                  @ModifiedBy
                                )
                                RETURNING *
                            )
                            INSERT INTO public.user_audit_log
                            (user_id, action_name, description, object_name, object_data, target_user_id)
                            SELECT
                            created_by, 'user_profile.add', 'added user profile', 'user_profile', row_to_json(new_user_profile), user_id
                            FROM new_user_profile
                            RETURNING target_user_id;
                        ";

        return conn.ExecuteScalarAsync<int>(query, new
        {
          UserId,
          Name,

                    CompanyName,
                    CompanyEmail,
                    CompanyWebsite,
                    CompanyDescription,
          

          ModifiedBy = modifiedBy,
        });
      });
    }

    public virtual Task<int> UpdateProfile(
            int UserId,
            int tenantId,
            string Name,
                        string CompanyName = "N/A",
                        string CompanyEmail = null,
                        string CompanyWebsite = null,
                        string CompanyDescription = null,
            
            int modifiedBy = 0)
    {
      return WithConnection(conn =>
      {
        var tenantClause = "";
        if (tenantId > 0)
        {
          tenantClause = @" AND EXISTS(SELECT 1 FROM public.""user"" uu WHERE uu.id = user_id AND uu.tenant_id = @TenantId) ";
        }

        var query = $@"
                            WITH updated_user_profile AS (
	                            UPDATE public.""user_profile""
                                SET
                                  name = @Name,

                                  company_name = @CompanyName,
                                  company_email = @CompanyEmail,
                                  company_website = @CompanyWebsite,
                                  company_description = @CompanyDescription,


                                  updated_at = now(),
                                  updated_by = @ModifiedBy
	                            WHERE user_id = @UserId
                                {tenantClause}
	                            RETURNING *
                            )
                            INSERT INTO public.user_audit_log
                            (user_id, action_name, description, object_name, object_data, target_user_id)
                            SELECT
                            @ModifiedBy, 'user_profile.update', 'updated user profile', 'user_profile', row_to_json(updated_user_profile), user_id
                            FROM updated_user_profile;
                        ";

        return conn.ExecuteAsync(query, new
        {
          UserId,
          Name,

                    CompanyName,
                    CompanyEmail,
                    CompanyWebsite,
                    CompanyDescription,
          

          TenantId = tenantId,
          ModifiedBy = modifiedBy,
        });
      });
    }

    #endregion Profile

    #region helpers

    private string FilterClause(
      int tenantId
      , UserFilter filter = UserFilter.All
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
        case UserFilter.All:
          filtered = false;
          break;

        case UserFilter.Trashed:
          clause += $"WHERE a.deleted_at IS NOT NULL ";
          break;

        default:
          throw new NotImplementedException($"Unknown user filter: '{Convert.ToString(filter)}'");
      }

      if (!includeTrashed && filter != UserFilter.Trashed)
      {
        clause += $" {(filtered ? "AND" : "WHERE")} a.deleted_at IS NULL ";
        filtered = true;
      }

      if (tenantId > 0)
      {
        clause += $" {(filtered ? "AND" : "WHERE")} a.tenant_id = @TenantId ";
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

    private string SortClause(SortBy<UserColumn> sortBy)
    {
      string sortColum;
      switch (sortBy.Column)
      {
        case UserColumn.Id:
          sortColum = "id";
          break;

        case UserColumn.Username:
          sortColum = "username";
          break;

        case UserColumn.Password:
          sortColum = "password";
          break;

        case UserColumn.LastLogin:
          sortColum = "last_login";
          break;

        case UserColumn.LockoutEnd:
          sortColum = "lockout_end";
          break;

        case UserColumn.BanEnd:
          sortColum = "ban_end";
          break;

        case UserColumn.IsConfirmed:
          sortColum = "is_confirmed";
          break;

        case UserColumn.IsActive:
          sortColum = "is_active";
          break;

        case UserColumn.CreatedBy:
          sortColum = "created_by";
          break;

        case UserColumn.CreatedAt:
          sortColum = "created_at";
          break;

        case UserColumn.UpdatedBy:
          sortColum = "updated_by";
          break;

        case UserColumn.UpdatedAt:
          sortColum = "updated_at";
          break;

        case UserColumn.DeletedBy:
          sortColum = "deleted_by";
          break;

        case UserColumn.DeletedAt:
          sortColum = "deleted_at";
          break;

        default:
          throw new NotImplementedException($"Unknown user sort column: '{Convert.ToString(sortBy.Column)}'");
      }

      string sortOrder;
      switch (sortBy.Direction)
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
