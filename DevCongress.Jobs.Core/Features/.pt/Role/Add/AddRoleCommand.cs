using Enexure.MicroBus;
using FluentResults;
using Newtonsoft.Json;
using Plutonium.Reactor.Pipeline;
using Plutonium.Reactor.Pipeline.Crosscutting.Auth;
using System.Threading.Tasks;
using Plutonium.Reactor.Attributes;

namespace DevCongress.Jobs.Core.Features.Role.Add
{
  [AuthorizedMessage("role.add")]
  public class AddRoleCommand : ICommand, ICommandWithResult
  {
    public string Name { get; }
    public string Description { get; }
    public bool IsActive { get; }

    [JsonIgnore]
    public TaskCompletionSource<Result> Result { get; }

    public AddRoleCommand(
                    string Name,
                    string Description,
                    bool IsActive = true,

          TaskCompletionSource<Result> result = null)
    {
      this.Name = Name;
      this.Description = Description;
      this.IsActive = IsActive;

      Result = result;
    }
  }
}
