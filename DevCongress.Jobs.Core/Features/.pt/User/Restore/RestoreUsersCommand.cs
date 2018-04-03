using Enexure.MicroBus;
using FluentResults;
using Newtonsoft.Json;
using Plutonium.Reactor.Pipeline;
using Plutonium.Reactor.Pipeline.Crosscutting.Auth;
using System.Threading.Tasks;
using Plutonium.Reactor.Attributes;

namespace DevCongress.Jobs.Core.Features.User.Restore
{
  [AuthorizedMessage("user.restore")]
  public class RestoreUsersCommand : ICommand, ICommandWithResult
  {
    public int[] Ids { get; }

    [JsonIgnore]
    public TaskCompletionSource<Result> Result { get; }

    public RestoreUsersCommand(int[] ids, TaskCompletionSource<Result> result)
    {
      Ids = ids;
      Result = result;
    }
  }
}
