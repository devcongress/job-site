using Enexure.MicroBus;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using DevCongress.Jobs.Core.Domain.DTO;
using DevCongress.Jobs.Core.Domain.Repository;
using Plutonium.Reactor.Services.Auth.Token;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;


namespace DevCongress.Jobs.Core.Features.Auth.Token.Register
{
  internal partial class TokenRegisterQueryHandler : BaseTokenRegisterQueryHandler, IQueryHandler<TokenRegisterQuery, TokenRegisterQueryResult>
  {
    public TokenRegisterQueryHandler(IMicroBus microbus, ILogger<TokenRegisterQueryHandler> logger, IUserRepository userRepository, IMagicTokenRepository magicTokenRepository, IAuthTokenProvider tokenProvider, IHttpContextAccessor httpContextAccessor) : base(microbus, logger, userRepository, magicTokenRepository, tokenProvider, httpContextAccessor)
    {
    }
  }

  #region Double-Derived

  internal abstract class BaseTokenRegisterQueryHandler
  {
    protected readonly IMicroBus _microbus;
    protected readonly ILogger<TokenRegisterQueryHandler> _logger;
    protected readonly IUserRepository _userRepository;
    protected readonly IMagicTokenRepository _magicTokenRepository;
    protected readonly IAuthTokenProvider _tokenProvider;
    protected readonly IHttpContextAccessor _httpContextAccessor;

    protected BaseTokenRegisterQueryHandler(
        IMicroBus microbus,
        ILogger<TokenRegisterQueryHandler> logger,
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

    public virtual async Task<TokenRegisterQueryResult> Handle(TokenRegisterQuery query)
    {
      var ipAddress = _httpContextAccessor.HttpContext.Connection.RemoteIpAddress.ToString();

      var magicToken = await _magicTokenRepository.Find(query.Token).ConfigureAwait(false);
      if (magicToken == null
         || magicToken.Purpose != "registration"
         || magicToken.UsedAt != null
         || DateTimeOffset.Compare(magicToken.ExpiresAt, DateTimeOffset.Now) < 0)
      {
        return Plutonium.Reactor.Lib.Results.Results<TokenRegisterQueryResult>.Fail("Invalid Token");
      }

      if (new Random().Next(1, 100) <= 25)
      {
        _logger.LogInformation("Purging Magic Tokens");
        await _magicTokenRepository.Purge().ConfigureAwait(false);
      }

      var user = await _userRepository.FindByUsername(magicToken.Email).ConfigureAwait(false);
      if (user != null)
      {
        _logger.LogWarning("Attempting to register using a token for an already registered account {email}", magicToken.Email);
        await _magicTokenRepository.MarkAsUsed(magicToken.Token).ConfigureAwait(false);
        return Plutonium.Reactor.Lib.Results.Results<TokenRegisterQueryResult>.Fail("You have already registered an account. Please login.");
      }

      using (var scope = new TransactionScope(TransactionScopeOption.Required, TransactionScopeAsyncFlowOption.Enabled))
      {
        await _magicTokenRepository.MarkAsUsed(magicToken.Token).ConfigureAwait(false);

        var registrationDetails = JsonConvert.DeserializeObject<RegistrationDetails>(magicToken.Metadata);

        var userId = await _userRepository.Add(
                      TenantId: null,
                      Username: magicToken.Email,
                      Password: null,

                      LastLogin: null,
                      LockoutEnd: null,
                      BanEnd: null,
                      IsConfirmed: true,
                      IsActive: true,

                      modifiedBy: 1).ConfigureAwait(false);

        await _userRepository.AddProfile(
                    UserId: userId,
                    Name: registrationDetails.Name,
                                        CompanyEmail : registrationDetails.CompanyEmail,
                                        CompanyWebsite : registrationDetails.CompanyWebsite,
                                        CompanyDescription : registrationDetails.CompanyDescription,
                    

                    modifiedBy: userId).ConfigureAwait(false);

        await _userRepository.UpdateRoles(
                    userId,
                    new[] { 2 }, // basic_user role
                    modifiedBy: 1).ConfigureAwait(false);

        scope.Complete();
      }

      user = await _userRepository.FindByUsername(magicToken.Email).ConfigureAwait(false);
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

      return Plutonium.Reactor.Lib.Results.Results<TokenRegisterQueryResult>.Ok()
              .WithSuccess("Account registered successfully")
              .With(res =>
              {
                res.BearerToken = token;
              });
    }
  }

  #endregion Double-Derived
}
