using FluentResults;

namespace DevCongress.Jobs.Core.Features.User.List
{
    public class ListUsersQueryResult : ResultBase<ListUsersQueryResult>
    {
        public int Page { get; internal set; }
        public int Limit { get; internal set; }
        public int TotalCount { get; internal set; }
        public int TotalPageCount { get; internal set; }
        public bool IncludeTrashed { get; internal set; }
        public Domain.Model.User[] Users { get; internal set; }

        public ListUsersQueryResult()
        {
        }
    }
}