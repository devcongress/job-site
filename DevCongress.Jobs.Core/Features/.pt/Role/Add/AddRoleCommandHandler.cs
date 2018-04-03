using Enexure.MicroBus;
using FluentResults;
using DevCongress.Jobs.Core.Domain.Repository;
using Plutonium.Reactor.Services.Auth.User;
using System;
using System.Threading.Tasks;
using System.Transactions;

namespace DevCongress.Jobs.Core.Features.Role.Add
{
    internal class AddRoleCommandHandler : ICommandHandler<AddRoleCommand>
    {
        private readonly IAuthenticatedUserProvider _userProvider;
        private readonly IRoleRepository _roleRepository;

        public AddRoleCommandHandler(IAuthenticatedUserProvider userProvider, IRoleRepository roleRepository)
        {
            _userProvider = userProvider ?? throw new ArgumentNullException(nameof(userProvider));
            _roleRepository = roleRepository ?? throw new ArgumentNullException(nameof(roleRepository));
        }

        public async Task Handle(AddRoleCommand command)
        {
            using (var scope = new TransactionScope(TransactionScopeOption.Required, TransactionScopeAsyncFlowOption.Enabled))
            {
                await _roleRepository.Add(
                              Name: command.Name,
                              Description: command.Description,
                              IsActive: command.IsActive,

                   modifiedBy: _userProvider.GetUser().Id).ConfigureAwait(false);

                scope.Complete();
            }

            command.Result.SetResult(Results.Ok().WithSuccess("Role added successfully"));
        }
    }
}