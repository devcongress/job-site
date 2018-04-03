using Enexure.MicroBus;
using FluentResults;
using DevCongress.Jobs.Core.Domain.Repository;
using Plutonium.Reactor.Services.Auth.User;
using System;
using System.Threading.Tasks;
using System.Transactions;

namespace DevCongress.Jobs.Core.Features.Role.Trash
{
    internal class TrashRolesCommandHandler : ICommandHandler<TrashRolesCommand>
    {
        private readonly IAuthenticatedUserProvider _userProvider;
        private readonly IRoleRepository _roleRepository;

        public TrashRolesCommandHandler(IAuthenticatedUserProvider userProvider, IRoleRepository roleRepository)
        {
            _userProvider = userProvider ?? throw new ArgumentNullException(nameof(userProvider));
            _roleRepository = roleRepository ?? throw new ArgumentNullException(nameof(roleRepository));
        }

        public async Task Handle(TrashRolesCommand command)
        {
            int count = 0;

            using (var scope = new TransactionScope(TransactionScopeOption.Required, TransactionScopeAsyncFlowOption.Enabled))
            {
                count = await _roleRepository.Trash(command.Ids, _userProvider.GetUser().Id).ConfigureAwait(false);

                scope.Complete();
            }

            command.Result.SetResult(Results.Ok().WithSuccess($"Trashed {count} {(count == 1 ? "role" : "roles")}"));
        }
    }
}