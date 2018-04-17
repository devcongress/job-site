using Enexure.MicroBus;
using FluentResults;
using DevCongress.Jobs.Core.Domain.Repository;
using DevCongress.Jobs.Core.Features.User.Fetch;
using Plutonium.Reactor.Pipeline.Crosscutting.Auth.Exception;
using Plutonium.Reactor.Services.Auth.User;
using System;
using System.Threading.Tasks;
using System.Transactions;


namespace DevCongress.Jobs.Core.Features.User.UpdateProfile
{
  internal partial class UpdateUserProfileCommandHandler : BaseUpdateUserProfileCommandHandler, ICommandHandler<UpdateUserProfileCommand>
  {
    public UpdateUserProfileCommandHandler(
        IMicroBus microbus,
        IAuthenticatedUserProvider userProvider,
        IUserRepository userRepository) : base(microbus, userProvider, userRepository)
    {
    }
  }

  #region Double-Derived

  internal abstract class BaseUpdateUserProfileCommandHandler
  {
    protected readonly IMicroBus _microbus;
    protected readonly IAuthenticatedUserProvider _userProvider;
    protected readonly IUserRepository _userRepository;

    protected BaseUpdateUserProfileCommandHandler(
        IMicroBus microbus,
        IAuthenticatedUserProvider userProvider,
        IUserRepository userRepository)
    {
      _microbus = microbus ?? throw new ArgumentNullException(nameof(microbus));
      _userProvider = userProvider ?? throw new ArgumentNullException(nameof(userProvider));
      _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
    }

    public virtual async Task Handle(UpdateUserProfileCommand command)
    {
      var user = await GetUser(command).ConfigureAwait(false);
      if (user is null)
      {
        return;
      }

      await UpdateUserProfile(user, command).ConfigureAwait(false);

      command.Result.SetResult(Results.Ok().WithSuccess("User profile updated successfully"));
    }

    protected virtual async Task UpdateUserProfile(Domain.Model.DetailedUser user, UpdateUserProfileCommand command)
    {
      using (var scope = new TransactionScope(TransactionScopeOption.Required, TransactionScopeAsyncFlowOption.Enabled))
      {
        var tenantId = _userProvider.GetUser().HasPermissions("user.profile.update.su") ? 0 : _userProvider.GetUser().TenantId;
        await _userRepository.UpdateProfile(
            user.Id,
            tenantId,
            Name: command.Name,
                        CompanyEmail :command.CompanyEmail,
                        CompanyWebsite :command.CompanyWebsite,
                        CompanyDescription :command.CompanyDescription,
            

            modifiedBy: _userProvider.GetUser().Id).ConfigureAwait(false);

        scope.Complete();
      }
    }

    protected virtual async Task<Domain.Model.DetailedUser> GetUser(UpdateUserProfileCommand command)
    {
      var fetchQuery = new FetchUserQuery(command.UserId);
      var fetchResult = await _microbus.QueryAsync(fetchQuery).ConfigureAwait(false);

      if (fetchResult.IsFailed)
      {
        command.Result.SetResult(Results.Merge(fetchResult));
        return null;
      }

      var user = fetchResult.User;

      var currentUser = _userProvider.GetUser();
      if (user.Id == currentUser.Id || currentUser.HasRoles("user.profile.update"))
      {
        var tenantId = currentUser.HasPermissions("user.profile.update.su") ? 0 : currentUser.TenantId;
        if (tenantId > 0 && user.Id != currentUser.Id && user.TenantId != tenantId)
        {
          throw new UnAuthorizedRequestException();
        }
      }
      else
      {
        throw new UnAuthorizedRequestException();
      }

      if (user.DeletedAt != null)
      {
        command.Result.SetResult(Results.Fail("Sorry, you cannot update a trashed user"));
        return null;
      }

      return user;
    }
  }

  #endregion Double-Derived
}
