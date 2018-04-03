namespace DevCongress.Jobs.Core.ViewModels.Auth
{
  public partial class PasswordLoginViewModel
  {
    public bool IsSuccess { get; internal set; }

    public string Username { get; internal set; }

    public bool Remember { get; internal set; }
  }
}
