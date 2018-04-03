using System;
using System.Collections.Generic;
using System.Text;

namespace DevCongress.Jobs.Core.ViewModels.Auth
{
  public partial class TokenLoginViewModel
  {
    public bool IsSuccess { get; internal set; }

    public string Email { get; internal set; }
    public bool Remember { get; internal set; }
  }
}
