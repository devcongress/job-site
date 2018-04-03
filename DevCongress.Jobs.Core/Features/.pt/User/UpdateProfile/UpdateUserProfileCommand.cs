using Enexure.MicroBus;
using FluentResults;
using Newtonsoft.Json;
using Plutonium.Reactor.Pipeline;
using Plutonium.Reactor.Pipeline.Crosscutting.Auth;
using System;
using System.Threading.Tasks;
using Plutonium.Reactor.Attributes;


namespace DevCongress.Jobs.Core.Features.User.UpdateProfile
{
  [AuthorizedMessage("!")]
  public partial class UpdateUserProfileCommand : ICommand, ICommandWithResult
  {
    public int UserId { get; }
    public string Name { get; }
        public string CompanyName { get; }
        public string CompanyEmail { get; }
        public string CompanyWebsite { get; }
        public string CompanyDescription { get; }
    

    [JsonIgnore]
    public TaskCompletionSource<Result> Result { get; }

    public UpdateUserProfileCommand(
          int UserId,
          string Name,
                    string CompanyName = "N/A",
                    string CompanyEmail = null,
                    string CompanyWebsite = null,
                    string CompanyDescription = null,
          

          TaskCompletionSource<Result> result = null)
    {
      this.UserId = UserId;
      this.Name = Name;
            this.CompanyName = CompanyName;
            this.CompanyEmail = CompanyEmail;
            this.CompanyWebsite = CompanyWebsite;
            this.CompanyDescription = CompanyDescription;
      

      Result = result;
    }
  }
}
