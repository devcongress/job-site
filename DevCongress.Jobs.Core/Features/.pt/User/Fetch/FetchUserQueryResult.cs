using FluentResults;
using static DevCongress.Jobs.Core.Domain.Model.User;

namespace DevCongress.Jobs.Core.Features.User.Fetch
{
    public class FetchUserQueryResult : ResultBase<FetchUserQueryResult>
    {
        public Domain.Model.DetailedUser User { get; internal set; }
        public UserAuditLog[] AuditLogs { get; internal set; }

        public FetchUserQueryResult()
        {
        }
    }
}