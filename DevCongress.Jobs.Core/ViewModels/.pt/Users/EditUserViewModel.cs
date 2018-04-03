using DevCongress.Jobs.Core.Domain.Model;

namespace DevCongress.Jobs.Core.ViewModels.Users
{
  public partial class EditUserViewModel
  {
    public DetailedUser User { get; internal set; }
    public Role[] Roles { get; internal set; }

    public bool IsSuccess { get; internal set; }
  }
}
