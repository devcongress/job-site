using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace DevCongress.Jobs.Core.Domain.DTO
{
    public partial class RegistrationDetails
    {
        public string Name { get; set; }
        public string CompanyEmail { get; set; }
        public string CompanyWebsite { get; set; }
        public string CompanyDescription { get; set; }
    }

    internal partial class RegistrationDetailsValidator : BaseRegistrationDetailsValidator
    {
        public RegistrationDetailsValidator() : base()
        {
        }
    }

    #region Double-Derived

    internal abstract class BaseRegistrationDetailsValidator : AbstractValidator<RegistrationDetails>
    {
        protected BaseRegistrationDetailsValidator()
        {
            AddDefaultRules();
            AddCustomRules();
        }

        protected virtual void AddDefaultRules()
        {
            RuleFor(request => request.CompanyEmail).NotEmpty();

            RuleFor(request => request.CompanyEmail).EmailAddress();

            RuleFor(request => request.CompanyEmail).Length(fields => 0, fields => 60);

            RuleFor(request => request.CompanyWebsite).Length(fields => 0, fields => 60);

            RuleFor(request => request.CompanyDescription).NotEmpty();

            RuleFor(request => request.CompanyDescription).Length(fields => 0, fields => 500);
        }

        protected virtual void AddCustomRules()
        {
        }
    }

    #endregion Double-Derived
}