using FluentResults;

namespace DevCongress.Jobs.Core.Features.Auth.Token.Register
{
  public class TokenRegisterQueryResult : ResultBase<TokenRegisterQueryResult>
  {
    public string BearerToken { get; internal set; }

    public TokenRegisterQueryResult()
    {
    }
  }
}
