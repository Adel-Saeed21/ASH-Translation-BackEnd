using ASH_Translation.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ASH_Translation.Data
{
    public class AppDbContext:IdentityDbContext<AdminUser>
    {
        public AppDbContext(DbContextOptions contextOptions)
            :base(contextOptions)
        {
            
        }
        public DbSet<Order> Orders { get; set; }
        public DbSet<AdminUser> Admins { get; set; }
        public DbSet<PasswordResetOtp> PasswordResetOtps { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.Entity<Order>().Property(order => order.PreferredContact)
                .HasConversion<string>();
            builder.Entity<Order>().Property(order => order.OrderStatus)
                .HasConversion<string>();
            
            // Configure RefreshToken relationships
            builder.Entity<RefreshToken>()
                .HasOne(rt => rt.User)
                .WithMany()
                .HasForeignKey(rt => rt.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }

    }
}
