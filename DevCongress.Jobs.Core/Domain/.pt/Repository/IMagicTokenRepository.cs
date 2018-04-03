using Dapper;
using MassTransit;
using Microsoft.Extensions.Options;
using Npgsql;
using DevCongress.Jobs.Core.Domain.Model;
using Plutonium.Reactor.Data;
using Plutonium.Reactor.Data.Query.Filter;
using Plutonium.Reactor.Data.Query.Sort;
using Plutonium.Reactor.Options;
using Plutonium.Reactor.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static DevCongress.Jobs.Core.Domain.Model.MagicToken;

namespace DevCongress.Jobs.Core.Domain.Repository
{
  internal partial interface IMagicTokenRepository
  {
    Task<MagicToken> Find(Guid Token);

    Task<Guid> Add(
            string Email,
            string Metadata,
            string Purpose,
            DateTimeOffset ExpiresAt);

    Task<int> MarkAsUsed(Guid Token);

    Task<int> Purge();
  }

  internal partial class MagicTokenRepository : BaseMagicTokenRepository, IMagicTokenRepository
  {
    public MagicTokenRepository(IOptions<ConnectionString> options, IConnectionResolver<NpgsqlConnection> resolver) : base(options, resolver)
    {
    }
  }

  #region Double-Derived

  internal abstract class BaseMagicTokenRepository : BasePgRepository
  {
    protected BaseMagicTokenRepository(IOptions<ConnectionString> options, IConnectionResolver<NpgsqlConnection> resolver) : base(options, resolver)
    {
    }

    public virtual Task<MagicToken> Find(Guid Token)
    {
      return WithConnection(conn =>
      {
        const string query = @"SELECT
	                              a.*
                                  FROM public.""magic_token"" a
                                  WHERE a.token = @Token
                              ";

        return conn.QuerySingleOrDefaultAsync<MagicToken>(query, new
        {
          Token,
        });
      });
    }

    public virtual Task<Guid> Add(
            string Email,
            string Metadata,
            string Purpose,
            DateTimeOffset ExpiresAt)
    {
      return WithConnection(conn =>
      {
        const string query = @"

                                INSERT INTO public.""magic_token""
                                (
                                  email,
                                  token,
                                  metadata,
                                  purpose,
                                  expires_at
                                )
                                VALUES
                                (
                                  @Email,
                                  @Token,
                                  @Metadata::jsonb,
                                  @Purpose,
                                  @ExpiresAt
                                )
                                RETURNING token;
                        ";

        return conn.ExecuteScalarAsync<Guid>(query, new
        {
          Token = NewId.NextGuid(),
          Email,
          Metadata,
          Purpose,
          ExpiresAt,
        });
      });
    }

    public virtual Task<int> MarkAsUsed(Guid Token)
    {
      return WithConnection(conn =>
      {
        const string query = @"
	                            UPDATE public.""magic_token""
                                SET
                                  used_at = now()
	                            WHERE token = @Token;
                        ";

        return conn.ExecuteAsync(query, new
        {
          Token,
        });
      });
    }

    public virtual Task<int> Purge()
    {
      return WithConnection(conn =>
      {
        const string query = @"
	                            DELETE FROM public.""magic_token""
                                    WHERE used_at <= @PurgeSince
                                    OR expires_at <= @PurgeSince
                        ";

        return conn.ExecuteAsync(query, new
        {
          PurgeSince = DateTimeOffset.Now.AddDays(-7),
        });
      });
    }
  }

  #endregion Double-Derived
}
