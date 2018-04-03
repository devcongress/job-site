using Newtonsoft.Json;
using System;

namespace DevCongress.Jobs.Core.Domain.Model
{
  public partial class User
  {
    public int Id { get; set; }

    public string Username { get; set; }

    [JsonIgnore]
    public string Password { get; set; }

    public DateTimeOffset? LastLogin { get; set; }

    public DateTimeOffset? LockoutEnd { get; set; }

    public DateTimeOffset? BanEnd { get; set; }

    public bool IsConfirmed { get; set; }

    public bool IsActive { get; set; }

    public int TenantId { get; set; }

    public int CreatedBy { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public int UpdatedBy { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public int? DeletedBy { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }

    public partial class UserAuditLog
    {
      public long Id { get; set; }
      public int UserId { get; set; }
      public string Username { get; set; }
      public long TargetUserId { get; set; }
      public string ActionName { get; set; }
      public string Description { get; set; }
      public string ObjectName { get; set; }
      public string ObjectData { get; set; }
      public DateTimeOffset CreatedAt { get; set; }
    }

    public enum UserFilter
    {
      All = 0,
      Trashed = 1,
    }

    public enum UserColumn
    {
      Id,

      Username,
      Password,
      LastLogin,
      LockoutEnd,
      BanEnd,
      IsConfirmed,
      IsActive,

      CreatedBy,
      CreatedAt,
      UpdatedBy,
      UpdatedAt,
      DeletedBy,
      DeletedAt,
    }
  }

  public partial class DetailedUser : User
  {
    public DetailedUserProfile Profile { get; set; }
    public Role[] Roles { get; set; }
  }
}
