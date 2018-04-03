using FluentValidation;

namespace DevCongress.Jobs.Core.Features.Role.GetLogs
{
  internal class GetRoleLogsQueryValidator : AbstractValidator<GetRoleLogsQuery>
  {
    public GetRoleLogsQueryValidator()
    {
      RuleFor(request => request.Id).GreaterThan(0);
      RuleFor(request => request.Limit).GreaterThanOrEqualTo(0);
    }
  }
}
