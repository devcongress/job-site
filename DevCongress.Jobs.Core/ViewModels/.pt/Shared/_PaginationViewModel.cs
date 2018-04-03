namespace DevCongress.Jobs.Core.ViewModels.Shared
{
  public partial class _PaginationViewModel
  {
    public long Count { get; set; }
    public long TotalCount { get; set; }
    public int Page { get; set; }
    public long TotalPageCount { get; set; }
    public int Limit { get; set; }
  }
}
