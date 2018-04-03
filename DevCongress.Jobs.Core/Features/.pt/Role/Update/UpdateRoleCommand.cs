using Enexure.MicroBus;
using FluentResults;
using Newtonsoft.Json;
using Plutonium.Reactor.Pipeline;
using Plutonium.Reactor.Pipeline.Crosscutting.Auth;
using System.Threading.Tasks;
using Plutonium.Reactor.Attributes;

namespace DevCongress.Jobs.Core.Features.Role.Update
{
  [AuthorizedMessage("role.update")]
  public class UpdateRoleCommand : ICommand, ICommandWithResult
  {
    public int Id { get; }

    public string Name { get; }
    public string Description { get; }
    public bool IsActive { get; }

    [JsonIgnore]
    public TaskCompletionSource<Result> Result { get; }

    public UpdateRoleCommand(int id,
                    string Name,
                    string Description,
                    bool IsActive = true,

          TaskCompletionSource<Result> result = null)
    {
      Id = id;
      this.Name = Name;
      this.Description = Description;
      this.IsActive = IsActive;

      Result = result;
    }
  }
}
