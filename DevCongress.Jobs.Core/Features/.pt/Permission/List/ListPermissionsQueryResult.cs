using FluentResults;

namespace DevCongress.Jobs.Core.Features.Permission.List
{
    public class ListPermissionsQueryResult : ResultBase<ListPermissionsQueryResult>
    {
        public Domain.Model.Permission[] Permissions { get; internal set; }

        public ListPermissionsQueryResult()
        {
        }
    }
}