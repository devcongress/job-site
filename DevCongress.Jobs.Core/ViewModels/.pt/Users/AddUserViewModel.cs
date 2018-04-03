namespace DevCongress.Jobs.Core.ViewModels.Users
{
  public partial class AddUserViewModel
  {
    public bool IsSuccess { get; internal set; }

    public string Username { get; internal set; }
    public string Password { get; internal set; }
    public string ConfirmPassword { get; internal set; }
  }
}
