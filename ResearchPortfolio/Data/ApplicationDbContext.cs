using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ResearchPortfolio.Models;

namespace ResearchPortfolio.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public DbSet<Publication> Publications { get; set; }
        public DbSet<Reference> References { get; set; }
        public DbSet<AuthorPublication> AuthorPublications { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<AuthorPublication>()
                .HasKey(ap => new { ap.UserId, ap.PublicationId });
        }
    }
}
