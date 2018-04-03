using DevCongress.Jobs.Core.Domain.Model;
using static DevCongress.Jobs.Core.Domain.Model.User;

namespace DevCongress.Jobs.Core.ViewModels.Users
{
  public partial class ViewUserViewModel
  {
    public DetailedUser User { get; internal set; }
    public UserAuditLog[] AuditLogs { get; internal set; }

    public bool IsSuccess { get; internal set; }
  }
}
