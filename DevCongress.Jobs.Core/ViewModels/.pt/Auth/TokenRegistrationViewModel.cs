
namespace DevCongress.Jobs.Core.ViewModels.Auth
{
    public partial class TokenRegistrationViewModel
    {
        public bool IsSuccess { get; internal set; }

        public string Email { get; internal set; }
        public string Name { get; internal set; }
                public string CompanyName { get; internal set; }
                public string CompanyEmail { get; internal set; }
                public string CompanyWebsite { get; internal set; }
                public string CompanyDescription { get; internal set; }
        
    }
}
