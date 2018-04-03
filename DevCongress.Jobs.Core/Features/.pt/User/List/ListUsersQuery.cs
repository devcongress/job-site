using Enexure.MicroBus;
using Plutonium.Reactor.Data.Query.Filter;
using Plutonium.Reactor.Data.Query.Sort;
using Plutonium.Reactor.Pipeline.Crosscutting.Auth;
using static DevCongress.Jobs.Core.Domain.Model.User;
using Plutonium.Reactor.Attributes;

namespace DevCongress.Jobs.Core.Features.User.List
{
  [AuthorizedMessage("user.list")]
  public class ListUsersQuery : IQuery<ListUsersQuery, ListUsersQueryResult>
  {
    public int Page { get; }
    public int Limit { get; }
    public UserFilter Filter { get; }
    public DateFilter CreateDateFilter { get; }
    public DateFilter UpdateDateFilter { get; }
    public DateFilter DeleteDateFilter { get; }
    public SortBy<UserColumn> SortBy { get; }
    public bool IncludeTrashed { get; }

    public ListUsersQuery(
      int page = 1
      , int limit = 10
      , UserFilter filter = UserFilter.All
      , DateFilter createDateFilter = default(DateFilter)
      , DateFilter updateDateFilter = default(DateFilter)
      , DateFilter deleteDateFilter = default(DateFilter)
      , SortBy<UserColumn> sortBy = default(SortBy<UserColumn>)
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
