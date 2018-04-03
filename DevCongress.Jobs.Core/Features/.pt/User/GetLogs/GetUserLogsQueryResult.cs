using FluentResults;
using static DevCongress.Jobs.Core.Domain.Model.User;

namespace DevCongress.Jobs.Core.Features.User.GetLogs
{
    public class GetUserLogsQueryResult : ResultBase<GetUserLogsQueryResult>
    {
        public UserAuditLog[] Logs { get; internal set; }

        public GetUserLogsQueryResult()
        {
        }
    }
}