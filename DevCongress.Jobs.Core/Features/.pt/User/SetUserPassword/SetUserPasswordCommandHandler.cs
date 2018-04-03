using Enexure.MicroBus;
using FluentResults;
using DevCongress.Jobs.Core.Domain.Repository;
using DevCongress.Jobs.Core.Features.User.Fetch;
using Plutonium.Reactor.Pipeline.Crosscutting.Auth.Exception;
using Plutonium.Reactor.Services.Auth.User;
using System;
using System.Threading.Tasks;
using System.Transactions;

namespace DevCongress.Jobs.Core.Features.User.SetUserPassword
{
  internal class SetUserPasswordCommandHandler : ICommandHandler<SetUserPasswordCommand>
  {
    private readonly IMicroBus _microbus;
    private readonly IAuthenticatedUserProvider _userProvider;
    private readonly IUserRepository _userRepository;

    public SetUserPasswordCommandHandler(IMicroBus microbus, IAuthenticatedUserProvider userProvider, IUserRepository userRepository)
    {
      _microbus = microbus ?? throw new ArgumentNullException(nameof(microbus));
      _userProvider = userProvider ?? throw new ArgumentNullException(nameof(userProvider));
      _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
    }

    public async Task Handle(SetUserPasswordCommand command)
    {
      var fetchQuery = new FetchUserQuery(command.Id);
      var fetchResult = await _microbus.QueryAsync(fetchQuery).ConfigureAwait(false);
      if (fetchResult.IsFailed)
      {
        command.Result.SetResult(Results.Merge(fetchResult));
        return;
      }
      var user = fetchResult.User;

      var currentUser = _userProvider.GetUser();
      if (!currentUser.HasPermissions("user.password.set.su") && user.TenantId != _userProvider.GetUser().TenantId)
      {
        throw new UnAuthorizedRequestException();
      }

      if (user.DeletedAt != null)
      {
        command.Result.SetResult(Results.Fail("Sorry, you cannot update a trashed user"));
        return;
      }

      using (var scope = new TransactionScope(TransactionScopeOption.Required, TransactionScopeAsyncFlowOption.Enabled))
      {
        var tenantId = _userProvider.GetUser().HasPermissions("user.password.set.su") ? 0 : _userProvider.GetUser().TenantId;
        await _userRepository.ChangePassword(
            user.Id,
            tenantId,
            Password: command.Password,

            modifiedBy: _userProvider.GetUser().Id).ConfigureAwait(false);

        scope.Complete();
      }

      command.Result.SetResult(Results.Ok().WithSuccess("Password changed successfully"));
    }
  }
}
