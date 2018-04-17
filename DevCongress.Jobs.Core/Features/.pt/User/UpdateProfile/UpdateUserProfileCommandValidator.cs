using FluentValidation;
using System.Linq;

namespace DevCongress.Jobs.Core.Features.User.UpdateProfile
{
  internal partial class UpdateUserProfileCommandValidator : BaseUpdateUserProfileCommandValidator
  {
    public UpdateUserProfileCommandValidator() : base()
    {
    }
  }

  #region Double-Derived

  internal abstract class BaseUpdateUserProfileCommandValidator : AbstractValidator<UpdateUserProfileCommand>
  {
    protected BaseUpdateUserProfileCommandValidator()
    {
      RuleFor(request => request.Result).NotNull();

      AddDefaultRules();
      AddCustomRules();
    }

    protected virtual void AddDefaultRules()
    {
      RuleFor(request => request.UserId).GreaterThan(0);
      RuleFor(request => request.Name).Length(fields => 1, fields => 50);
      RuleFor(request => request.Name).NotEmpty();

      
      

                        
            RuleFor(request => request.CompanyEmail).NotEmpty();
      
                        
            RuleFor(request => request.CompanyEmail).EmailAddress();
      
                        
            RuleFor(request => request.CompanyEmail).Length(fields => 0, fields => 60);
      
                                    
            RuleFor(request => request.CompanyWebsite).NotEmpty();
      
                        
            RuleFor(request => request.CompanyWebsite).Length(fields => 0, fields => 60);
      
                                    
            RuleFor(request => request.CompanyTwitter).Length(fields => 0, fields => 60);
      
                                    
            RuleFor(request => request.CompanyDescription).NotEmpty();
      
                        
            RuleFor(request => request.CompanyDescription).Length(fields => 0, fields => 500);
      
                      }

    protected virtual void AddCustomRules()
    {
    }
  }

  #endregion Double-Derived
}
