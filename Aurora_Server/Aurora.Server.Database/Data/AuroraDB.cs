using Microsoft.EntityFrameworkCore;
using Aurora.Server.Database.Models;

namespace Aurora.Server.Database.Data
{
    public class AuroraDB : DbContext
    {
        public DbSet<User> Users { get; set; } = null!;
        public DbSet<Post> Posts { get; set; } = null!;
        public DbSet<Comment> Comments { get; set; } = null!;
        public DbSet<Reaction> Reactions { get; set; } = null!;
        public DbSet<Follow> Follows { get; set; } = null!;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                // Directly set the connection string here
                string connectionString = "Server=Ariel\\SQLEXPRESS;Database=Aurora.Server.Database;Trusted_Connection=True;TrustServerCertificate=True;";
                optionsBuilder.UseSqlServer(connectionString);
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>()
                .HasKey(u => u.UserID);

            modelBuilder.Entity<Post>()
                .HasKey(p => p.PostId);

            modelBuilder.Entity<Comment>()
                .HasKey(c => c.CommentID);

            modelBuilder.Entity<Reaction>()
                .HasKey(r => r.ReactionID);

            modelBuilder.Entity<Follow>()
                .HasKey(f => f.RelationID);

            base.OnModelCreating(modelBuilder);
        }
    }
}
