using Enexure.MicroBus;
using FluentResults;
using DevCongress.Jobs.Core.Domain.Repository;
using DevCongress.Jobs.Core.Features.Role.Fetch;
using Plutonium.Reactor.Services.Auth.User;
using System;
using System.Threading.Tasks;
using System.Transactions;

namespace DevCongress.Jobs.Core.Features.Role.UpdatePermissions
{
    internal class UpdateRolePermissionsCommandHandler : ICommandHandler<UpdateRolePermissionsCommand>
    {
        private readonly IMicroBus _microbus;
        private readonly IAuthenticatedUserProvider _userProvider;
        private readonly IRoleRepository _roleRepository;

        public UpdateRolePermissionsCommandHandler(IMicroBus microbus, IAuthenticatedUserProvider userProvider, IRoleRepository roleRepository)
        {
            _microbus = microbus ?? throw new ArgumentNullException(nameof(microbus));
            _userProvider = userProvider ?? throw new ArgumentNullException(nameof(userProvider));
            _roleRepository = roleRepository ?? throw new ArgumentNullException(nameof(roleRepository));
        }

        public async Task Handle(UpdateRolePermissionsCommand command)
        {
            var fetchQuery = new FetchRoleQuery(command.Id);
            var fetchResult = await _microbus.QueryAsync(fetchQuery).ConfigureAwait(false);

            if (fetchResult.IsFailed)
            {
                command.Result.SetResult(Results.Merge(fetchResult));
                return;
            }

            var role = fetchResult.Role;

            if (role.DeletedAt != null)
            {
                command.Result.SetResult(Results.Fail("Sorry, you cannot update a trashed role"));
                return;
            }

            using (var scope = new TransactionScope(TransactionScopeOption.Required, TransactionScopeAsyncFlowOption.Enabled))
            {
                await _roleRepository.UpdatePermissions(
                    role.Id,
                    PermissionIds: command.Permissions,

                    modifiedBy: _userProvider.GetUser().Id).ConfigureAwait(false);

                scope.Complete();
            }

            command.Result.SetResult(Results.Ok().WithSuccess("Role permissions updated successfully"));
        }
    }
}