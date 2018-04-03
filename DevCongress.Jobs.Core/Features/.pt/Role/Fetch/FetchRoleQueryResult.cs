using FluentResults;
using static DevCongress.Jobs.Core.Domain.Model.Role;

namespace DevCongress.Jobs.Core.Features.Role.Fetch
{
    public class FetchRoleQueryResult : ResultBase<FetchRoleQueryResult>
    {
        public Domain.Model.DetailedRole Role { get; internal set; }
        public RoleAuditLog[] AuditLogs { get; internal set; }

        public FetchRoleQueryResult()
        {
        }
    }
}