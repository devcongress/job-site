using DevCongress.Jobs.Core.Domain.Model;

namespace DevCongress.Jobs.Core.ViewModels.Users
{
  public partial class ListUsersViewModel
  {
    public User[] Users { get; internal set; }

    public int Page { get; internal set; }
    public int TotalPageCount { get; internal set; }

    public int TotalCount { get; internal set; }
    public int Limit { get; internal set; }

    public string SortOrder { get; internal set; }
    public string SortBy { get; internal set; }

    public bool IncludeTrashed { get; internal set; }

    public bool IsSuccess { get; internal set; }
  }
}
