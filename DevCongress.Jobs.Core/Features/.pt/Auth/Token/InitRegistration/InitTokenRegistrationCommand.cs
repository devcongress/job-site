using Enexure.MicroBus;
using FluentResults;
using Newtonsoft.Json;
using Plutonium.Reactor.Pipeline;
using Plutonium.Reactor.Pipeline.Crosscutting.Auth;
using System.Threading.Tasks;
using System;
using Plutonium.Reactor.Attributes;
using DevCongress.Jobs.Core.Domain.Model;
using DevCongress.Jobs.Core.Domain.DTO;

namespace DevCongress.Jobs.Core.Features.Auth.Token.InitRegistration
{
  [AuthorizedMessage("?")]
  public class InitTokenRegistrationCommand : ICommand, ICommandWithResult
  {
    public string Email { get; }
    public string ConfirmUrl { get; }
    public RegistrationDetails RegistrationDetails { get; }

    [JsonIgnore]
    public TaskCompletionSource<Result> Result { get; }

    public InitTokenRegistrationCommand(
        string email,
        string confirmUrl,
        RegistrationDetails registrationDetails,
        TaskCompletionSource<Result> result)
    {
      Email = email;
      ConfirmUrl = confirmUrl;
      RegistrationDetails = registrationDetails;
      Result = result;
    }
  }
}
