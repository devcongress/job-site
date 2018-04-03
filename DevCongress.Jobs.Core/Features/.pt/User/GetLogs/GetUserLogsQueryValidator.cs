using FluentValidation;

namespace DevCongress.Jobs.Core.Features.User.GetLogs
{
  internal class GetUserLogsQueryValidator : AbstractValidator<GetUserLogsQuery>
  {
    public GetUserLogsQueryValidator()
    {
      RuleFor(request => request.Id).GreaterThan(0);
      RuleFor(request => request.Limit).GreaterThanOrEqualTo(0);
    }
  }
}
