using Enexure.MicroBus;
using DevCongress.Jobs.Core.Domain.Repository;
using System.Linq;
using System.Threading.Tasks;

namespace DevCongress.Jobs.Core.Features.Role.GetLogs
{
  internal class GetRoleLogsQueryHandler : IQueryHandler<GetRoleLogsQuery, GetRoleLogsQueryResult>
  {
    private readonly IRoleRepository _roleRepository;

    public GetRoleLogsQueryHandler(IRoleRepository roleRepository)
    {
      _roleRepository = roleRepository;
    }

    public async Task<GetRoleLogsQueryResult> Handle(GetRoleLogsQuery query)
    {
      var role = await _roleRepository.Fetch(query.Id).ConfigureAwait(false);
      if (role == null)
      {
        return Plutonium.Reactor.Lib.Results.Results<GetRoleLogsQueryResult>.Fail("Role does not exist");
      }

      var logs = await _roleRepository.GetLogs(query.Id, query.Limit).ConfigureAwait(false);

      return Plutonium.Reactor.Lib.Results.Results<GetRoleLogsQueryResult>.Ok()
              .WithSuccess("Retrieved role successfully")
              .With(res => res.Logs = logs.ToArray());
    }
  }
}
