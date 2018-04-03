using Enexure.MicroBus;
using Plutonium.Reactor.Attributes;

namespace DevCongress.Jobs.Core.Features.Auth.Password.Login
{
  [AuthorizedMessage("?")]
  public class PasswordLoginQuery : IQuery<PasswordLoginQuery, PasswordLoginQueryResult>
  {
    public string Username { get; }
    public string Password { get; }

    public PasswordLoginQuery(
        string Username,
        string Password)
    {
      this.Username = Username;
      this.Password = Password;
    }
  }
}
