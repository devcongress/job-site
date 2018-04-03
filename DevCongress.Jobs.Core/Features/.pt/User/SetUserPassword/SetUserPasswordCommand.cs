using Enexure.MicroBus;
using FluentResults;
using Newtonsoft.Json;
using Plutonium.Reactor.Pipeline;
using Plutonium.Reactor.Pipeline.Crosscutting.Auth;
using System.Threading.Tasks;
using Plutonium.Reactor.Attributes;

namespace DevCongress.Jobs.Core.Features.User.SetUserPassword
{
  [AuthorizedMessage("user.password.set")]
  public class SetUserPasswordCommand : ICommand, ICommandWithResult
  {
    public int Id { get; }
    public string Password { get; }
    public string ConfirmPassword { get; }

    [JsonIgnore]
    public TaskCompletionSource<Result> Result { get; }

    public SetUserPasswordCommand(
        int id,
        string Password,
        string ConfirmPassword,
        TaskCompletionSource<Result> result)
    {
      Id = id;
      this.Password = Password;
      this.ConfirmPassword = ConfirmPassword;
      Result = result;
    }
  }
}
