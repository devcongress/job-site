using Enexure.MicroBus;
using DevCongress.Jobs.Core.Domain.Repository;
using Plutonium.Reactor.Pipeline.Crosscutting.Auth.Exception;
using Plutonium.Reactor.Services.Auth.User;
using System.Linq;
using System.Threading.Tasks;

namespace DevCongress.Jobs.Core.Features.User.Fetch
{
  internal class FetchUserQueryHandler : IQueryHandler<FetchUserQuery, FetchUserQueryResult>
  {
    private readonly IAuthenticatedUserProvider _userProvider;
    private readonly IUserRepository _userRepository;

    public FetchUserQueryHandler(
        IAuthenticatedUserProvider userProvider,
        IUserRepository userRepository)
    {
      _userProvider = userProvider;
      _userRepository = userRepository;
    }

    public async Task<FetchUserQueryResult> Handle(FetchUserQuery query)
    {
      Domain.Model.DetailedUser user = null;

      var currentUser = _userProvider.GetUser();
      if (query.Id == currentUser.Id || currentUser.HasRoles("user.fetch"))
      {
        var tenantId = currentUser.HasRoles("user.fetch.su") ? 0 : currentUser.TenantId;
        user = await _userRepository.Fetch(query.Id, tenantId).ConfigureAwait(false);
      }
      else
      {
        throw new UnAuthorizedRequestException();
      }

      if (user == null)
      {
        return Plutonium.Reactor.Lib.Results.Results<FetchUserQueryResult>.Fail("User does not exist");
      }

      user.Profile = query.IncludeDetailedProperties
          ? (await _userRepository.FetchProfile(user.Id).ConfigureAwait(false))
          : null;

      user.Roles = query.IncludeDetailedProperties
          ? (await _userRepository.GetRoles(user.Id).ConfigureAwait(false)).ToArray()
          : null;

      var logsBy = currentUser.HasAnyPermissions("user.getLogs.su") ? 0 : currentUser.Id;
      var logs = query.IncludeLogs
          ? (await _userRepository.GetLogs(query.Id, 50, logsBy).ConfigureAwait(false)).ToArray()
          : null;

      return Plutonium.Reactor.Lib.Results.Results<FetchUserQueryResult>.Ok()
              .WithSuccess("Retrieved user successfully")
              .With(res =>
              {
                res.User = user;
                res.AuditLogs = logs?.ToArray();
              });
    }
  }
}
