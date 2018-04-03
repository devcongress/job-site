using FluentValidation;
using System.Linq;
using System;

namespace DevCongress.Jobs.Core.Features.Auth.Token.InitLogin
{
  internal class InitTokenLoginCommandValidator : AbstractValidator<InitTokenLoginCommand>
  {
    public InitTokenLoginCommandValidator()
    {
      RuleFor(request => request.EmailAddress).NotEmpty();
      RuleFor(request => request.EmailAddress).EmailAddress();

      RuleFor(request => request.ConfirmUrl).NotEmpty();

      RuleFor(request => request.Result).NotNull();
    }
  }
}
