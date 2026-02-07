using Microsoft.AspNetCore.Identity;

namespace ResearchPortfolio.Models
{
    public class ApplicationUser : IdentityUser
    {
        public ICollection<AuthorPublication> AuthorPublications { get; set; }
    }
}
