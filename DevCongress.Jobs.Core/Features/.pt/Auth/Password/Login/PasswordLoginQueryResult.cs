using FluentResults;

namespace DevCongress.Jobs.Core.Features.Auth.Password.Login
{
  public class PasswordLoginQueryResult : ResultBase<PasswordLoginQueryResult>
  {
    public string BearerToken { get; internal set; }

    public PasswordLoginQueryResult()
    {
    }
  }
}
