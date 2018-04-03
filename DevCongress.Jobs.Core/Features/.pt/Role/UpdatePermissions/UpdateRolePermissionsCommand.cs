using Enexure.MicroBus;
using FluentResults;
using Newtonsoft.Json;
using Plutonium.Reactor.Pipeline;
using Plutonium.Reactor.Pipeline.Crosscutting.Auth;
using System.Threading.Tasks;
using Plutonium.Reactor.Attributes;

namespace DevCongress.Jobs.Core.Features.Role.UpdatePermissions
{
  [AuthorizedMessage("role.permissions.update")]
  public class UpdateRolePermissionsCommand : ICommand, ICommandWithResult
  {
    public int Id { get; }

    public int[] Permissions { get; }

    [JsonIgnore]
    public TaskCompletionSource<Result> Result { get; }

    public UpdateRolePermissionsCommand(int id,
                    int[] Permissions,

          TaskCompletionSource<Result> result = null)
    {
      Id = id;
      this.Permissions = Permissions;

      Result = result;
    }
  }
}
