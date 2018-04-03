using Enexure.MicroBus;
using FluentResults;
using Newtonsoft.Json;
using Plutonium.Reactor.Pipeline;
using System.Threading.Tasks;
using Plutonium.Reactor.Attributes;

namespace DevCongress.Jobs.Core.Features.User.Add
{
  [AuthorizedMessage("user.add")]
  public class AddUserCommand : ICommand, ICommandWithResult
  {
    public int? TenantId { get; }
    public string Username { get; }
    public string Password { get; }
    public string ConfirmPassword { get; }

    [JsonIgnore]
    public TaskCompletionSource<Result> Result { get; }

    public AddUserCommand(
                    int? TenantId,
                    string Username,
                    string Password,
                    string ConfirmPassword,

          TaskCompletionSource<Result> result = null)
    {
      this.TenantId = TenantId;
      this.Username = Username;
      this.Password = Password;
      this.ConfirmPassword = ConfirmPassword;

      Result = result;
    }
  }
}
