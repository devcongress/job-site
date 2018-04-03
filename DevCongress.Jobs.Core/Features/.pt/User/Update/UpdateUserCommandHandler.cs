using Enexure.MicroBus;
using FluentResults;
using DevCongress.Jobs.Core.Domain.Repository;
using DevCongress.Jobs.Core.Features.User.Fetch;
using Plutonium.Reactor.Services.Auth.User;
using System;
using System.Threading.Tasks;
using System.Transactions;

namespace DevCongress.Jobs.Core.Features.User.Update
{
  internal class UpdateUserCommandHandler : ICommandHandler<UpdateUserCommand>
  {
    private readonly IMicroBus _microbus;
    private readonly IAuthenticatedUserProvider _userProvider;
    private readonly IUserRepository _userRepository;

    public UpdateUserCommandHandler(IMicroBus microbus, IAuthenticatedUserProvider userProvider, IUserRepository userRepository)
    {
      _microbus = microbus ?? throw new ArgumentNullException(nameof(microbus));
      _userProvider = userProvider ?? throw new ArgumentNullException(nameof(userProvider));
      _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
    }

    public async Task Handle(UpdateUserCommand command)
    {
      var fetchQuery = new FetchUserQuery(command.Id);
      var fetchResult = await _microbus.QueryAsync(fetchQuery).ConfigureAwait(false);

      if (fetchResult.IsFailed)
      {
        command.Result.SetResult(Results.Merge(fetchResult));
        return;
      }

      var user = fetchResult.User;

      if (user.DeletedAt != null)
      {
        command.Result.SetResult(Results.Fail("Sorry, you cannot update a trashed user"));
        return;
      }

      using (var scope = new TransactionScope(TransactionScopeOption.Required, TransactionScopeAsyncFlowOption.Enabled))
      {
        var tenantId = _userProvider.GetUser().HasPermissions("user.update.su") ? 0 : _userProvider.GetUser().TenantId;
        await _userRepository.Update(
            user.Id,
            tenantId,
            Username: command.Username,
            LastLogin: user.LastLogin,
            LockoutEnd: user.LockoutEnd,
            BanEnd: command.BanEnd,
            IsConfirmed: command.IsConfirmed,
            IsActive: command.IsActive,

            modifiedBy: _userProvider.GetUser().Id).ConfigureAwait(false);

        scope.Complete();
      }

      command.Result.SetResult(Results.Ok().WithSuccess("User updated successfully"));
    }
  }
}
