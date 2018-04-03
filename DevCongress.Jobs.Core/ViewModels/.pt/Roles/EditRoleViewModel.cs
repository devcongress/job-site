using DevCongress.Jobs.Core.Domain.Model;

namespace DevCongress.Jobs.Core.ViewModels.Roles
{
  public partial class EditRoleViewModel
  {
    public DetailedRole Role { get; internal set; }
    public Permission[] Permissions { get; internal set; }

    public bool IsSuccess { get; internal set; }
  }
}
