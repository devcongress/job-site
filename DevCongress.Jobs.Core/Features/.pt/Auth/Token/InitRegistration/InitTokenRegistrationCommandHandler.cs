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

namespace DevCongress.Jobs.Core.Features.Auth.Token.InitRegistration
{
  internal class InitTokenRegistrationCommandHandler : ICommandHandler<InitTokenRegistrationCommand>
  {
    private readonly IMicroBus _microbus;
    private readonly ILogger<InitTokenRegistrationCommandHandler> _logger;
    private readonly IUserRepository _userRepository;
    private readonly IMagicTokenRepository _magicTokenRepository;
    private readonly ITemplateRenderer _templateRenderer;
    private readonly IEmailSender _emailSender;

    public InitTokenRegistrationCommandHandler(
        IMicroBus microbus,
        ILogger<InitTokenRegistrationCommandHandler> logger,
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

    public async Task Handle(InitTokenRegistrationCommand command)
    {
      var existingUser = await _userRepository.FindByUsername(command.Email).ConfigureAwait(false);
      if (existingUser != null)
      {
        command.Result.SetResult(Results.Fail("Email address is already taken."));
        return;
      }

      var success = false;

      using (var scope = new TransactionScope(TransactionScopeOption.Required, TransactionScopeAsyncFlowOption.Enabled))
      {
        var token = await _magicTokenRepository.Add(
            command.Email,
            JsonConvert.SerializeObject(command.RegistrationDetails),
            "registration",
            DateTimeOffset.Now.AddDays(1)
          ).ConfigureAwait(false);

        var model = new
        {
          Token = token,
          ConfirmUrl = $"{command.ConfirmUrl}?Token={token}",
        };
        var rendered = await _templateRenderer.Render("Email/Register", model).ConfigureAwait(false);

        var response =
          await _emailSender.SendEmail(
            new Message("Complete Registration", rendered),
            new[] { new Address(command.Email) }
          ).ConfigureAwait(false);

        if (response.IsSuccess)
        {
          scope.Complete();

          _logger.LogInformation("Registration init: {emailAddress}", command.Email);
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
