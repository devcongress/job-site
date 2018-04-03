using Enexure.MicroBus;
using FluentResults;
using DevCongress.Jobs.Core.Domain.Repository;
using Plutonium.Reactor.Services.Auth.User;
using System;
using System.Threading.Tasks;
using System.Transactions;

namespace DevCongress.Jobs.Core.Features.User.Trash
{
  internal class TrashUsersCommandHandler : ICommandHandler<TrashUsersCommand>
  {
    private readonly IAuthenticatedUserProvider _userProvider;
    private readonly IUserRepository _userRepository;

    public TrashUsersCommandHandler(IAuthenticatedUserProvider userProvider, IUserRepository userRepository)
    {
      _userProvider = userProvider ?? throw new ArgumentNullException(nameof(userProvider));
      _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
    }

    public async Task Handle(TrashUsersCommand command)
    {
      int count = 0;

      using (var scope = new TransactionScope(TransactionScopeOption.Required, TransactionScopeAsyncFlowOption.Enabled))
      {
        var tenantId = _userProvider.GetUser().HasPermissions("user.trash.su") ? 0 : _userProvider.GetUser().TenantId;
        count = await _userRepository.Trash(
            command.Ids,
            tenantId,
            _userProvider.GetUser().Id).ConfigureAwait(false);

        scope.Complete();
      }

      command.Result.SetResult(Results.Ok().WithSuccess($"Trashed {count} {(count == 1 ? "user" : "users")}"));
    }
  }
}
