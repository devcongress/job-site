using Dapper;
using Microsoft.Extensions.Options;
using Npgsql;
using DevCongress.Jobs.Core.Domain.Model;
using Plutonium.Reactor.Data;
using Plutonium.Reactor.Data.Query.Sort;
using Plutonium.Reactor.Options;
using Plutonium.Reactor.Repository;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static DevCongress.Jobs.Core.Domain.Model.Permission;

namespace DevCongress.Jobs.Core.Domain.Repository
{
  internal partial interface IPermissionRepository
  {
    Task<IEnumerable<Permission>> List(SortBy<PermissionColumn> sortBy = default(SortBy<PermissionColumn>));
  }

  internal partial class PermissionRepository : BasePermissionRepository, IPermissionRepository
  {
    public PermissionRepository(IOptions<ConnectionString> options, IConnectionResolver<NpgsqlConnection> resolver) : base(options, resolver)
    {
    }
  }

  #region Double-Derived

  internal abstract class BasePermissionRepository : BasePgRepository
  {
    protected BasePermissionRepository(IOptions<ConnectionString> options, IConnectionResolver<NpgsqlConnection> resolver) : base(options, resolver)
    {
    }

    public virtual Task<IEnumerable<Permission>> List(SortBy<PermissionColumn> sortBy = default(SortBy<PermissionColumn>))
    {
      return WithConnection(conn =>
      {
        var query = $@"SELECT
	                        a.*
                            FROM public.""permission"" a
                            ORDER BY {SortClause(sortBy)}
                  ";

        return conn.QueryAsync<Permission>(query);
      });
    }

    #region helpers

    private string SortClause(SortBy<PermissionColumn> sortBy)
    {
      string sortColum;
      switch (sortBy?.Column ?? PermissionColumn.Id)
      {
        case PermissionColumn.Id:
          sortColum = "id";
          break;

        case PermissionColumn.Name:
          sortColum = "name";
          break;

        case PermissionColumn.Description:
          sortColum = "description";
          break;

        case PermissionColumn.CreatedAt:
          sortColum = "created_at";
          break;

        default:
          throw new NotImplementedException($"Unknown permission sort column: '{Convert.ToString(sortBy.Column)}'");
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
