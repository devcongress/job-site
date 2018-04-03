using Enexure.MicroBus;
using FluentResults;
using Newtonsoft.Json;
using Plutonium.Reactor.Pipeline;
using Plutonium.Reactor.Pipeline.Crosscutting.Auth;
using System;
using System.Threading.Tasks;
using Plutonium.Reactor.Attributes;

namespace DevCongress.Jobs.Core.Features.User.Update
{
  [AuthorizedMessage("user.update")]
  public class UpdateUserCommand : ICommand, ICommandWithResult
  {
    public int Id { get; }

    public string Username { get; }
    public DateTimeOffset? BanEnd { get; }
    public bool IsConfirmed { get; }
    public bool IsActive { get; }

    [JsonIgnore]
    public TaskCompletionSource<Result> Result { get; }

    public UpdateUserCommand(int id,
                    string Username,
                    DateTimeOffset? BanEnd = null,
                    bool IsConfirmed = false,
                    bool IsActive = true,

          TaskCompletionSource<Result> result = null)
    {
      Id = id;
      this.Username = Username;
      this.BanEnd = BanEnd;
      this.IsConfirmed = IsConfirmed;
      this.IsActive = IsActive;

      Result = result;
    }
  }
}
