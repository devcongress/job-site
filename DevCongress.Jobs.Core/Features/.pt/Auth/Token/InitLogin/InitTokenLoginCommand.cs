using Enexure.MicroBus;
using FluentResults;
using Newtonsoft.Json;
using Plutonium.Reactor.Pipeline;
using Plutonium.Reactor.Pipeline.Crosscutting.Auth;
using System.Threading.Tasks;
using System;
using Plutonium.Reactor.Attributes;

namespace DevCongress.Jobs.Core.Features.Auth.Token.InitLogin
{
  [AuthorizedMessage("?")]
  public class InitTokenLoginCommand : ICommand, ICommandWithResult
  {
    public string EmailAddress { get; }
    public string ConfirmUrl { get; }
    public bool Remember { get; }

    [JsonIgnore]
    public TaskCompletionSource<Result> Result { get; }

    public InitTokenLoginCommand(
        string emailAddress,
        string confirmUrl,
        bool remember,
        TaskCompletionSource<Result> result)
    {
      EmailAddress = emailAddress;
      ConfirmUrl = confirmUrl;
      Remember = remember;
      Result = result;
    }
  }
}
