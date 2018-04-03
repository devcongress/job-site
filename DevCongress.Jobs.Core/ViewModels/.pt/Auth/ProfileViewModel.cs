using DevCongress.Jobs.Core.Domain.Model;
using System;

namespace DevCongress.Jobs.Core.ViewModels.Auth
{
  public partial class ProfileViewModel
  {
    public bool IsSuccess { get; internal set; }

    public DetailedUser User { get; internal set; }
    public User.UserAuditLog[] AuditLogs { get; internal set; }
  }
}
