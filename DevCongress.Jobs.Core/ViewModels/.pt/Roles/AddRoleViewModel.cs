namespace DevCongress.Jobs.Core.ViewModels.Roles
{
  public partial class AddRoleViewModel
  {
    public bool IsSuccess { get; internal set; }

    public string Name { get; internal set; }
    public string Description { get; internal set; }
    public bool IsActive { get; internal set; } = true;
  }
}
