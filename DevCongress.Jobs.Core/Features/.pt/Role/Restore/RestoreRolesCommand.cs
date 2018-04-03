using Enexure.MicroBus;
using FluentResults;
using Newtonsoft.Json;
using Plutonium.Reactor.Pipeline;
using System.Threading.Tasks;
using Plutonium.Reactor.Attributes;

namespace DevCongress.Jobs.Core.Features.Role.Restore
{
  [AuthorizedMessage("role.restore")]
  public class RestoreRolesCommand : ICommand, ICommandWithResult
  {
    public int[] Ids { get; }

    [JsonIgnore]
    public TaskCompletionSource<Result> Result { get; }

    public RestoreRolesCommand(int[] ids, TaskCompletionSource<Result> result)
    {
      Ids = ids;
      Result = result;
    }
  }
}
