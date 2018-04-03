using Enexure.MicroBus;
using Plutonium.Reactor.Attributes;
using System;

namespace DevCongress.Jobs.Core.Features.Auth.Token.Register
{
  [AuthorizedMessage("?")]
  public class TokenRegisterQuery : IQuery<TokenRegisterQuery, TokenRegisterQueryResult>
  {
    public Guid Token { get; }

    public TokenRegisterQuery(
        Guid Token)
    {
      this.Token = Token;
    }
  }
}
