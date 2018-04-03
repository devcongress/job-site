using Enexure.MicroBus;
using Plutonium.Reactor.Pipeline.Crosscutting.Auth;
using Plutonium.Reactor.Attributes;

namespace DevCongress.Jobs.Core.Features.Role.Fetch
{
  [AuthorizedMessage("role.fetch")]
  public class FetchRoleQuery : IQuery<FetchRoleQuery, FetchRoleQueryResult>
  {
    public int Id { get; }
    public bool IncludeLogs { get; }
    public bool IncludeDetailedProperties { get; }

    public FetchRoleQuery(int id, bool includeLogs = false, bool includeDetailedProperties = true)
    {
      Id = id;
      IncludeLogs = includeLogs;
      IncludeDetailedProperties = includeDetailedProperties;
    }
  }
}
