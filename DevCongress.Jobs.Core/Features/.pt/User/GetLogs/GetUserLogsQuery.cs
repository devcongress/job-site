using Enexure.MicroBus;
using Plutonium.Reactor.Attributes;
using Plutonium.Reactor.Pipeline.Crosscutting.Auth;

namespace DevCongress.Jobs.Core.Features.User.GetLogs
{
  [AuthorizedMessage("!")]
  public class GetUserLogsQuery : IQuery<GetUserLogsQuery, GetUserLogsQueryResult>
  {
    public int Id { get; }
    public int Limit { get; }

    public GetUserLogsQuery(int id, int limit)
    {
      Id = id;
      Limit = limit;
    }
  }
}
