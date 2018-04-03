using Enexure.MicroBus;
using Microsoft.AspNetCore.Http;
using DevCongress.Jobs.Core.Domain.Repository;
using Plutonium.Reactor.Services.Auth.Token;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;

namespace DevCongress.Jobs.Core.Features.Auth.Password.Login
{
  internal class PasswordLoginQueryHandler : IQueryHandler<PasswordLoginQuery, PasswordLoginQueryResult>
  {
    private readonly IMicroBus _microbus;
    private readonly IUserRepository _userRepository;
    private readonly IAuthTokenProvider _tokenProvider;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public PasswordLoginQueryHandler(IMicroBus microbus, IUserRepository userRepository, IAuthTokenProvider tokenProvider, IHttpContextAccessor httpContextAccessor)
    {
      _microbus = microbus ?? throw new ArgumentNullException(nameof(microbus));
      _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
      _tokenProvider = tokenProvider ?? throw new ArgumentNullException(nameof(tokenProvider));
      _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
    }

    public async Task<PasswordLoginQueryResult> Handle(PasswordLoginQuery query)
    {
      var ipAddress = _httpContextAccessor.HttpContext.Connection.RemoteIpAddress.ToString();

      var user = await _userRepository.FindByUsername(query.Username).ConfigureAwait(false);
      if (user == null || user.DeletedAt != null)
      {
        return Plutonium.Reactor.Lib.Results.Results<PasswordLoginQueryResult>.Fail("Invalid credentials");
      }

      if (user.LockoutEnd != null && DateTimeOffset.Compare(DateTimeOffset.Now, user.LockoutEnd.Value) < 0)
      {
        return Plutonium.Reactor.Lib.Results.Results<PasswordLoginQueryResult>.Fail("Account locked out. Try again in a few minutes");
      }

      if (!BCrypt.Net.BCrypt.Verify(query.Password, user.Password))
      {
        using (var scope = new TransactionScope(TransactionScopeOption.Required, TransactionScopeAsyncFlowOption.Enabled))
        {
          await _userRepository.LoginFailed(user.Id, ipAddress).ConfigureAwait(false);

          scope.Complete();
        }

        return Plutonium.Reactor.Lib.Results.Results<PasswordLoginQueryResult>.Fail("Invalid credentials");
      }

      if (!user.IsActive)
      {
        return Plutonium.Reactor.Lib.Results.Results<PasswordLoginQueryResult>.Fail("Account has been deactivated");
      }

      if (user.BanEnd != null && DateTimeOffset.Compare(DateTimeOffset.Now, user.BanEnd.Value) < 0)
      {
        return Plutonium.Reactor.Lib.Results.Results<PasswordLoginQueryResult>.Fail($"Account has been banned till {user.BanEnd}");
      }

      user.Profile = await _userRepository.FetchProfile(user.Id).ConfigureAwait(false);

      var roles = (await _userRepository.GetRoles(user.Id).ConfigureAwait(false)).Select(x => x.Name.ToLowerInvariant()).ToArray();
      var permissions = (await _userRepository.GetPermissions(user.Id).ConfigureAwait(false)).Select(x => x.Name.ToLowerInvariant()).ToArray();

      var bearerToken = _tokenProvider.GenerateToken(
         user.Id,
         user.Username,
         user.TenantId,
         user.Profile.Name,
         roles: roles,
         permissions: permissions,
         issuer: "example.com",
         ttlMins: 60 * 24);

      // todo : work on some refresh token flows

      using (var scope = new TransactionScope(TransactionScopeOption.Required, TransactionScopeAsyncFlowOption.Enabled))
      {
        await _userRepository.Login(user.Id, ipAddress).ConfigureAwait(false);

        scope.Complete();
      }

      return Plutonium.Reactor.Lib.Results.Results<PasswordLoginQueryResult>.Ok()
              .WithSuccess("Logged in successfully")
              .With(res => res.BearerToken = bearerToken);
    }
  }
}
