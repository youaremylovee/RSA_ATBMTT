using Microsoft.EntityFrameworkCore;
using RSA_UI.Models.Entity;

namespace RSA_UI.Repositories.Context
{
    public class RsaContext: DbContext
    {
        private RsaContext(){}
        public RsaContext(DbContextOptions<RsaContext> options): base(options)
        {
        }
        public DbSet<Company> Companies { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Company>().ToTable("Company");
        }
    }
}
