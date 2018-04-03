using Enexure.MicroBus;
using Plutonium.Reactor.Data.Query.Sort;
using Plutonium.Reactor.Pipeline.Crosscutting.Auth;
using static DevCongress.Jobs.Core.Domain.Model.Permission;
using Plutonium.Reactor.Attributes;

namespace DevCongress.Jobs.Core.Features.Permission.List
{
  [AuthorizedMessage("*")]
  public class ListPermissionsQuery : IQuery<ListPermissionsQuery, ListPermissionsQueryResult>
  {
    public SortBy<PermissionColumn> SortBy { get; }

    public ListPermissionsQuery(SortBy<PermissionColumn> sortBy = default(SortBy<PermissionColumn>))
    {
      SortBy = sortBy;
    }
  }
}
