using Enexure.MicroBus;
using FluentResults;
using Newtonsoft.Json;
using Plutonium.Reactor.Pipeline;
using Plutonium.Reactor.Pipeline.Crosscutting.Auth;
using System.Threading.Tasks;
using Plutonium.Reactor.Attributes;

namespace DevCongress.Jobs.Core.Features.Role.Trash
{
  [AuthorizedMessage("role.trash")]
  public class TrashRolesCommand : ICommand, ICommandWithResult
  {
    public int[] Ids { get; }

    [JsonIgnore]
    public TaskCompletionSource<Result> Result { get; }

    public TrashRolesCommand(int[] ids, TaskCompletionSource<Result> result)
    {
      Ids = ids;
      Result = result;
    }
  }
}
