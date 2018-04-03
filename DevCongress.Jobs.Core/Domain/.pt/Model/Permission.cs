using System;

namespace DevCongress.Jobs.Core.Domain.Model
{
  public partial class Permission
  {
    public int Id { get; set; }

    public string Name { get; set; }
    public string Description { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public enum PermissionColumn
    {
      Id,

      Name,
      Description,

      CreatedAt,
    }
  }
}
