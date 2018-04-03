using System;
using Newtonsoft.Json;

namespace DevCongress.Jobs.Core.Domain.Model
{
  public partial class MagicToken
  {
    public int Id { get; set; }

    public string Email { get; set; }

    public Guid Token { get; set; }

    public string Metadata { get; set; }

    public string Purpose { get; set; }

    public DateTimeOffset ExpiresAt { get; set; }

    public DateTimeOffset? UsedAt { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
  }
}
