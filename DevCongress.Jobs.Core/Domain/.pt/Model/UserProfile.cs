using System;


namespace DevCongress.Jobs.Core.Domain.Model
{
    public partial class UserProfile
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        public string Name { get; set; }

        
                public string CompanyName { get; set; }
        
                public string CompanyEmail { get; set; }
        
                public string CompanyWebsite { get; set; }
        
                public string CompanyDescription { get; set; }
        
        public int CreatedBy { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public int UpdatedBy { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }
    }

    public partial class DetailedUserProfile : UserProfile
    {
        // extended model properties go here. leave blank if not applicable
        // used in the fetch query
    }
}
