using FluentResults;
using static DevCongress.Jobs.Core.Domain.Model.Role;

namespace DevCongress.Jobs.Core.Features.Role.GetLogs
{
    public class GetRoleLogsQueryResult : ResultBase<GetRoleLogsQueryResult>
    {
        public RoleAuditLog[] Logs { get; internal set; }

        public GetRoleLogsQueryResult()
        {
        }
    }
}