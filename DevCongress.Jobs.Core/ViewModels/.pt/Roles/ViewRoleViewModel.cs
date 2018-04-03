using DevCongress.Jobs.Core.Domain.Model;
using static DevCongress.Jobs.Core.Domain.Model.Role;

namespace DevCongress.Jobs.Core.ViewModels.Roles
{
  public partial class ViewRoleViewModel
  {
    public DetailedRole Role { get; internal set; }
    public RoleAuditLog[] AuditLogs { get; internal set; }

    public bool IsSuccess { get; internal set; }
  }
}
