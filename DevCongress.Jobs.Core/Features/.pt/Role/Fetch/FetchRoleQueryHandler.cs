using Enexure.MicroBus;
using DevCongress.Jobs.Core.Domain.Repository;
using System.Linq;
using System.Threading.Tasks;

namespace DevCongress.Jobs.Core.Features.Role.Fetch
{
  internal class FetchRoleQueryHandler : IQueryHandler<FetchRoleQuery, FetchRoleQueryResult>
  {
    private readonly IRoleRepository _roleRepository;

    public FetchRoleQueryHandler(IRoleRepository roleRepository)
    {
      _roleRepository = roleRepository;
    }

    public async Task<FetchRoleQueryResult> Handle(FetchRoleQuery query)
    {
      var role = await _roleRepository.Fetch(query.Id).ConfigureAwait(false);
      if (role == null)
      {
        return Plutonium.Reactor.Lib.Results.Results<FetchRoleQueryResult>.Fail("Role does not exist");
      }

      role.Permissions = query.IncludeDetailedProperties
          ? (await _roleRepository.GetPermissions(role.Id).ConfigureAwait(false)).ToArray()
          : null;

      var logs = query.IncludeLogs
          ? (await _roleRepository.GetLogs(role.Id, 50).ConfigureAwait(false)).ToArray()
          : null;

      return Plutonium.Reactor.Lib.Results.Results<FetchRoleQueryResult>.Ok()
              .WithSuccess("Retrieved role successfully")
              .With(res =>
              {
                res.Role = role;
                res.AuditLogs = logs?.ToArray();
              });
    }
  }
}
