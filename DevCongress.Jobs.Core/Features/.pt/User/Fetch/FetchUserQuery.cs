using Enexure.MicroBus;
using Plutonium.Reactor.Pipeline.Crosscutting.Auth;
using Plutonium.Reactor.Attributes;

namespace DevCongress.Jobs.Core.Features.User.Fetch
{
  [AuthorizedMessage("!")]
  public class FetchUserQuery : IQuery<FetchUserQuery, FetchUserQueryResult>
  {
    public int Id { get; }
    public bool IncludeLogs { get; }
    public bool IncludeDetailedProperties { get; }

    public FetchUserQuery(int id, bool includeLogs = false, bool includeDetailedProperties = true)
    {
      Id = id;
      IncludeLogs = includeLogs;
      IncludeDetailedProperties = includeDetailedProperties;
    }
  }
}
