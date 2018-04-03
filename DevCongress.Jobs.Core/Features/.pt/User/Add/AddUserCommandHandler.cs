using Enexure.MicroBus;
using FluentResults;
using DevCongress.Jobs.Core.Domain.Repository;
using Plutonium.Reactor.Services.Auth.User;
using System;
using System.Threading.Tasks;
using System.Transactions;


namespace DevCongress.Jobs.Core.Features.User.Add
{
  internal class AddUserCommandHandler : ICommandHandler<AddUserCommand>
  {
    private readonly IAuthenticatedUserProvider _userProvider;
    private readonly IUserRepository _userRepository;

    public AddUserCommandHandler(IAuthenticatedUserProvider userProvider, IUserRepository userRepository)
    {
      _userProvider = userProvider ?? throw new ArgumentNullException(nameof(userProvider));
      _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
    }

    public async Task Handle(AddUserCommand command)
    {
      using (var scope = new TransactionScope(TransactionScopeOption.Required, TransactionScopeAsyncFlowOption.Enabled))
      {
        var currentUser = _userProvider.GetUser();
        var tenantId = command.TenantId != null && currentUser.HasRoles("user.add.su")
                        ? command.TenantId.Value
                        : currentUser.TenantId;

        var userId = await _userRepository.Add(
                              TenantId: tenantId,
                              Username: command.Username,
                              Password: command.Password,

                              LastLogin: null,
                              LockoutEnd: null,
                              BanEnd: null,
                              IsConfirmed: true,
                              IsActive: true,

                              modifiedBy: _userProvider.GetUser().Id).ConfigureAwait(false);

        await _userRepository.AddProfile(
                    UserId: userId,
                    Name: command.Username,
                                        CompanyName :"N/A",
                                        CompanyEmail :null,
                                        CompanyWebsite :null,
                                        CompanyDescription :null,
                    

                    modifiedBy: _userProvider.GetUser().Id).ConfigureAwait(false);

        scope.Complete();
      }

      command.Result.SetResult(Results.Ok().WithSuccess("User added successfully"));
    }
  }
}
