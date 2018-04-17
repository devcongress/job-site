using FluentValidation;
using System.Linq;
using System;
using DevCongress.Jobs.Core.Domain.DTO;

namespace DevCongress.Jobs.Core.Features.Auth.Token.InitRegistration
{
  internal partial class InitTokenRegistrationCommandValidator : BaseInitTokenRegistrationCommandValidator
  {
    public InitTokenRegistrationCommandValidator() : base()
    {
    }
  }

  #region Double-Derived

  internal abstract class BaseInitTokenRegistrationCommandValidator : AbstractValidator<InitTokenRegistrationCommand>
  {
    protected BaseInitTokenRegistrationCommandValidator()
    {
      RuleFor(request => request.Result).NotNull();

      AddDefaultRules();
      AddCustomRules();
    }

    protected virtual void AddDefaultRules()
    {
      RuleFor(request => request.Email).NotEmpty();
      RuleFor(request => request.Email).EmailAddress();

      RuleFor(request => request.ConfirmUrl).NotEmpty();

      RuleFor(request => request.RegistrationDetails).NotNull();
      RuleFor(request => request.RegistrationDetails).SetValidator(new RegistrationDetailsValidator());
    }

    protected virtual void AddCustomRules()
    {
    }
  }

  #endregion Double-Derived
}
