using Microsoft.EntityFrameworkCore;

namespace SocialBrothersApi.Models {
    public class AddressContext : DbContext
    {
        public AddressContext(DbContextOptions<AddressContext> options)
            : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            // connect to sqlite database
            options.UseSqlite("Data Source=addresses.db");
        }

        public DbSet<Address> Addresses { get; set; } = null!;
    }
}