using Enexure.MicroBus;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using DevCongress.Jobs.Core.Domain.Repository;
using Plutonium.Reactor.Services.Auth.Token;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;

namespace DevCongress.Jobs.Core.Features.Auth.Token.Login
{
  internal class TokenLoginQueryHandler : IQueryHandler<TokenLoginQuery, TokenLoginQueryResult>
  {
    private readonly IMicroBus _microbus;
    private readonly ILogger<TokenLoginQueryHandler> _logger;
    private readonly IUserRepository _userRepository;
    private readonly IMagicTokenRepository _magicTokenRepository;
    private readonly IAuthTokenProvider _tokenProvider;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public TokenLoginQueryHandler(
        IMicroBus microbus,
        ILogger<TokenLoginQueryHandler> logger,
        IUserRepository userRepository,
        IMagicTokenRepository magicTokenRepository,
        IAuthTokenProvider tokenProvider,
        IHttpContextAccessor httpContextAccessor)
    {
      _microbus = microbus ?? throw new ArgumentNullException(nameof(microbus));
      _logger = logger ?? throw new ArgumentNullException(nameof(logger));
      _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
      _magicTokenRepository = magicTokenRepository ?? throw new ArgumentNullException(nameof(magicTokenRepository));
      _tokenProvider = tokenProvider ?? throw new ArgumentNullException(nameof(tokenProvider));
      _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
    }

    public async Task<TokenLoginQueryResult> Handle(TokenLoginQuery query)
    {
      var ipAddress = _httpContextAccessor.HttpContext.Connection.RemoteIpAddress.ToString();

      var magicToken = await _magicTokenRepository.Find(query.Token).ConfigureAwait(false);
      if (magicToken == null
         || magicToken.Purpose != "login"
         || magicToken.UsedAt != null
         || DateTimeOffset.Compare(magicToken.ExpiresAt, DateTimeOffset.Now) < 0)
      {
        return Plutonium.Reactor.Lib.Results.Results<TokenLoginQueryResult>.Fail("Invalid Token");
      }
      await _magicTokenRepository.MarkAsUsed(magicToken.Token).ConfigureAwait(false);

      if (new Random().Next(1, 100) <= 25)
      {
        _logger.LogInformation("Purging Magic Tokens");
        await _magicTokenRepository.Purge().ConfigureAwait(false);
      }

      var user = await _userRepository.FindByUsername(magicToken.Email).ConfigureAwait(false);
      if (user == null || user.DeletedAt != null)
      {
        return Plutonium.Reactor.Lib.Results.Results<TokenLoginQueryResult>.Fail("Invalid user account");
      }

      if (user.LockoutEnd != null && DateTimeOffset.Compare(DateTimeOffset.Now, user.LockoutEnd.Value) < 0)
      {
        return Plutonium.Reactor.Lib.Results.Results<TokenLoginQueryResult>.Fail("Account locked out. Try again in a few minutes");
      }

      if (!user.IsActive)
      {
        return Plutonium.Reactor.Lib.Results.Results<TokenLoginQueryResult>.Fail("Account has been deactivated");
      }

      if (user.BanEnd != null && DateTimeOffset.Compare(DateTimeOffset.Now, user.BanEnd.Value) < 0)
      {
        return Plutonium.Reactor.Lib.Results.Results<TokenLoginQueryResult>.Fail($"Account has been banned till {user.BanEnd}");
      }

      user.Profile = await _userRepository.FetchProfile(user.Id).ConfigureAwait(false);

      var roles = (await _userRepository.GetRoles(user.Id).ConfigureAwait(false)).Select(x => x.Name.ToLowerInvariant()).ToArray();
      var permissions = (await _userRepository.GetPermissions(user.Id).ConfigureAwait(false)).Select(x => x.Name.ToLowerInvariant()).ToArray();

      var token = _tokenProvider.GenerateToken(
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

      return Plutonium.Reactor.Lib.Results.Results<TokenLoginQueryResult>.Ok()
              .WithSuccess("Logged in successfully")
              .With(res =>
              {
                res.BearerToken = token;
                res.Remember = JsonConvert.DeserializeObject<bool>(magicToken.Metadata);
              });
    }
  }
}
