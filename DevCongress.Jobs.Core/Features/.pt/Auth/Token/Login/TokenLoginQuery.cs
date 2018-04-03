using Enexure.MicroBus;
using Plutonium.Reactor.Attributes;
using System;

namespace DevCongress.Jobs.Core.Features.Auth.Token.Login
{
  [AuthorizedMessage("?")]
  public class TokenLoginQuery : IQuery<TokenLoginQuery, TokenLoginQueryResult>
  {
    public Guid Token { get; }

    public TokenLoginQuery(
        Guid Token)
    {
      this.Token = Token;
    }
  }
}
