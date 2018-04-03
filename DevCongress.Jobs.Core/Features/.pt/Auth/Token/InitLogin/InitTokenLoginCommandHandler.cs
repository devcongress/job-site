using Enexure.MicroBus;
using FluentResults;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using DevCongress.Jobs.Core.Domain.Repository;
using Plutonium.Reactor.Services.Auth.User;
using Plutonium.Reactor.Services.Messaging;
using Plutonium.Reactor.Services.Messaging.Email;
using Plutonium.Reactor.Services.TemplateRenderer;
using System;
using System.Threading.Tasks;
using System.Transactions;

namespace DevCongress.Jobs.Core.Features.Auth.Token.InitLogin
{
  internal class InitTokenLoginCommandHandler : ICommandHandler<InitTokenLoginCommand>
  {
    private readonly IMicroBus _microbus;
    private readonly ILogger<InitTokenLoginCommandHandler> _logger;
    private readonly IUserRepository _userRepository;
    private readonly IMagicTokenRepository _magicTokenRepository;
    private readonly ITemplateRenderer _templateRenderer;
    private readonly IEmailSender _emailSender;

    public InitTokenLoginCommandHandler(
        IMicroBus microbus,
        ILogger<InitTokenLoginCommandHandler> logger,
        IUserRepository userRepository,
        IMagicTokenRepository magicTokenRepository,
        ITemplateRenderer templateRenderer,
        IEmailSender emailSender)
    {
      _microbus = microbus ?? throw new ArgumentNullException(nameof(microbus));
      _logger = logger ?? throw new ArgumentNullException(nameof(logger));
      _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
      _magicTokenRepository = magicTokenRepository ?? throw new ArgumentNullException(nameof(magicTokenRepository));
      _templateRenderer = templateRenderer ?? throw new ArgumentNullException(nameof(templateRenderer));
      _emailSender = emailSender ?? throw new ArgumentNullException(nameof(emailSender));
    }

    public async Task Handle(InitTokenLoginCommand command)
    {
      var success = false;

      var user = await _userRepository.FindByUsername(command.EmailAddress).ConfigureAwait(false);
      if (user is null)
      {
        _logger.LogWarning("Failed token login init attempt: {emailAddress}", command.EmailAddress);
        command.Result.SetResult(Results.Fail("Unknown user account"));
        return;
      }

      using (var scope = new TransactionScope(TransactionScopeOption.Required, TransactionScopeAsyncFlowOption.Enabled))
      {
        var token = await _magicTokenRepository.Add(
          command.EmailAddress,
          JsonConvert.SerializeObject(command.Remember),
          "login",
          DateTimeOffset.Now.AddDays(1)
          ).ConfigureAwait(false);

        var model = new
        {
          Token = token,
          ConfirmUrl = $"{command.ConfirmUrl}?Token={token}",
        };
        var rendered = await _templateRenderer.Render("Email/Login", model).ConfigureAwait(false);

        var response =
          await _emailSender.SendEmail(
            new Message("Login Token", rendered),
            new[] { new Address(command.EmailAddress) }
          ).ConfigureAwait(false);

        if (response.IsSuccess)
        {
          scope.Complete();
          success = true;
        }
      }

      if (success)
        command.Result.SetResult(Results.Ok().WithSuccess("Please check your email for your token"));
      else
        command.Result.SetResult(Results.Fail("Something went wrong. Please try again."));
    }
  }
}
