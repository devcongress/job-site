using System;

namespace DevCongress.Jobs.Core.Domain.Model
{
  public partial class Role
  {
    public int Id { get; set; }

    public string Name { get; set; }

    public string Description { get; set; }

    public bool IsActive { get; set; }

    public int CreatedBy { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public int UpdatedBy { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public int? DeletedBy { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }

    public partial class RoleAuditLog
    {
      public long Id { get; set; }
      public int UserId { get; set; }
      public string Username { get; set; }
      public long TargetRoleId { get; set; }
      public string ActionName { get; set; }
      public string Description { get; set; }
      public string ObjectName { get; set; }
      public string ObjectData { get; set; }
      public DateTimeOffset CreatedAt { get; set; }
    }

    public enum RoleFilter
    {
      All = 0,
      Trashed = 1,
    }

    public enum RoleColumn
    {
      Id,

      Name,
      Description,
      IsActive,

      CreatedBy,
      CreatedAt,
      UpdatedBy,
      UpdatedAt,
      DeletedBy,
      DeletedAt,
    }
  }

  public partial class DetailedRole : Role
  {
    public Permission[] Permissions { get; set; }
  }
}
