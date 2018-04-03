using Enexure.MicroBus;
using FluentResults;
using Newtonsoft.Json;
using Plutonium.Reactor.Pipeline;
using Plutonium.Reactor.Pipeline.Crosscutting.Auth;
using System.Threading.Tasks;
using Plutonium.Reactor.Attributes;

namespace DevCongress.Jobs.Core.Features.User.UpdateRoles
{
  [AuthorizedMessage("user.roles.update")]
  public class UpdateUserRolesCommand : ICommand, ICommandWithResult
  {
    public int Id { get; }

    public int[] RoleIds { get; }

    [JsonIgnore]
    public TaskCompletionSource<Result> Result { get; }

    public UpdateUserRolesCommand(int id,
                    int[] RoleIds,

          TaskCompletionSource<Result> result = null)
    {
      Id = id;
      this.RoleIds = RoleIds;

      Result = result;
    }
  }
}
