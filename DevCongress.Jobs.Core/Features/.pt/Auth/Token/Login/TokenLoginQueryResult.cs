using FluentResults;

namespace DevCongress.Jobs.Core.Features.Auth.Token.Login
{
  public class TokenLoginQueryResult : ResultBase<TokenLoginQueryResult>
  {
    public string BearerToken { get; internal set; }
    public bool Remember { get; internal set; }

    public TokenLoginQueryResult()
    {
    }
  }
}
