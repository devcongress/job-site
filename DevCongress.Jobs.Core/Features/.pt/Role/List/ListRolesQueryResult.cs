using FluentResults;

namespace DevCongress.Jobs.Core.Features.Role.List
{
    public class ListRolesQueryResult : ResultBase<ListRolesQueryResult>
    {
        public int Page { get; internal set; }
        public int Limit { get; internal set; }
        public int TotalCount { get; internal set; }
        public int TotalPageCount { get; internal set; }
        public bool IncludeTrashed { get; internal set; }
        public Domain.Model.Role[] Roles { get; internal set; }

        public ListRolesQueryResult()
        {
        }
    }
}