using Enexure.MicroBus;
using Plutonium.Reactor.Pipeline.Crosscutting.Auth;
using Plutonium.Reactor.Attributes;

namespace DevCongress.Jobs.Core.Features.Role.GetLogs
{
  [AuthorizedMessage("role.getlogs")]
  public class GetRoleLogsQuery : IQuery<GetRoleLogsQuery, GetRoleLogsQueryResult>
  {
    public int Id { get; }
    public int Limit { get; }

    public GetRoleLogsQuery(int id, int limit)
    {
      Id = id;
      Limit = limit;
    }
  }
}
