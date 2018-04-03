using Enexure.MicroBus;
using DevCongress.Jobs.Core.Domain.Repository;
using DevCongress.Jobs.Core.Features.User.Fetch;
using Plutonium.Reactor.Pipeline.Crosscutting.Auth.Exception;
using Plutonium.Reactor.Services.Auth.User;
using System.Linq;
using System.Threading.Tasks;

namespace DevCongress.Jobs.Core.Features.User.GetLogs
{
  internal class GetUserLogsQueryHandler : IQueryHandler<GetUserLogsQuery, GetUserLogsQueryResult>
  {
    private readonly IMicroBus _microbus;
    private readonly IAuthenticatedUserProvider _userProvider;
    private readonly IUserRepository _userRepository;

    public GetUserLogsQueryHandler(
        IMicroBus microbus,
        IAuthenticatedUserProvider userProvider,
        IUserRepository userRepository)
    {
      _microbus = microbus;
      _userProvider = userProvider;
      _userRepository = userRepository;
    }

    public async Task<GetUserLogsQueryResult> Handle(GetUserLogsQuery query)
    {
      var fetchQuery = new FetchUserQuery(query.Id);
      var fetchResult = await _microbus.QueryAsync(fetchQuery).ConfigureAwait(false);
      if (fetchResult.IsFailed)
      {
        return Plutonium.Reactor.Lib.Results.Results<GetUserLogsQueryResult>.Merge(fetchResult);
      }

      var user = fetchResult.User;

      var currentUser = _userProvider.GetUser();
      if (query.Id == currentUser.Id || currentUser.HasRoles("user.getLogs"))
      {
        var tenantId = currentUser.HasPermissions("user.getLogs.su") ? 0 : currentUser.TenantId;
        if (tenantId > 0 && user.Id != currentUser.Id && user.TenantId != tenantId)
        {
          throw new UnAuthorizedRequestException();
        }
      }
      else
      {
        throw new UnAuthorizedRequestException();
      }

      var logsBy = currentUser.HasAnyPermissions("user.getLogs", "user.getLogs.su") ? 0 : currentUser.Id;
      var logs = await _userRepository.GetLogs(query.Id, query.Limit, logsBy).ConfigureAwait(false);

      return Plutonium.Reactor.Lib.Results.Results<GetUserLogsQueryResult>.Ok()
              .WithSuccess("Retrieved logs successfully")
              .With(res => res.Logs = logs.ToArray());
    }
  }
}
