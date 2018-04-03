using Enexure.MicroBus;
using Plutonium.Reactor.Data.Query.Filter;
using Plutonium.Reactor.Data.Query.Sort;
using Plutonium.Reactor.Pipeline.Crosscutting.Auth;
using static DevCongress.Jobs.Core.Domain.Model.Role;
using Plutonium.Reactor.Attributes;

namespace DevCongress.Jobs.Core.Features.Role.List
{
  [AuthorizedMessage("role.list")]
  public class ListRolesQuery : IQuery<ListRolesQuery, ListRolesQueryResult>
  {
    public int Page { get; }
    public int Limit { get; }
    public RoleFilter Filter { get; }
    public DateFilter CreateDateFilter { get; }
    public DateFilter UpdateDateFilter { get; }
    public DateFilter DeleteDateFilter { get; }
    public SortBy<RoleColumn> SortBy { get; }
    public bool IncludeTrashed { get; }

    public ListRolesQuery(
      int page = 1
      , int limit = 10
      , RoleFilter filter = RoleFilter.All
      , DateFilter createDateFilter = default(DateFilter)
      , DateFilter updateDateFilter = default(DateFilter)
      , DateFilter deleteDateFilter = default(DateFilter)
      , SortBy<RoleColumn> sortBy = default(SortBy<RoleColumn>)
      , bool includeTrashed = false)
    {
      Page = page;
      Limit = limit;
      Filter = filter;
      CreateDateFilter = createDateFilter;
      UpdateDateFilter = updateDateFilter;
      DeleteDateFilter = deleteDateFilter;
      SortBy = sortBy;
      IncludeTrashed = includeTrashed;
    }
  }
}
