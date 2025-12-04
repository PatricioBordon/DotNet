using Microsoft.EntityFrameworkCore;
using BookManagementApi.Models;

namespace BookManagementApi.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Book> Books { get; set; } = null!;
        public DbSet<Author> Authors { get; set; } = null!;
        public DbSet<User> Users { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Book>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
                entity.Property(e => e.Isbn).IsRequired();
                entity.Property(e => e.Title).IsRequired();
                entity.Property(e => e.PublicationYear).IsRequired();
                
                entity.HasOne(e => e.Author)
                    .WithMany(a => a.Books)
                    .HasForeignKey(e => e.AuthorId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Author>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
                entity.Property(e => e.Name).IsRequired();
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
                entity.Property(e => e.Username).IsRequired();
                entity.Property(e => e.HashedPassword).IsRequired();
                entity.HasIndex(e => e.Username).IsUnique();
            });
        }
    }

    public static class DbInitializer
    {
        public static void Initialize(ApplicationDbContext context)
        {
            if (!context.Users.Any())
            {
                var users = new List<User>
                {
                    new User
                    {
                        Id = Guid.NewGuid(),
                        Username = "admin",
                        HashedPassword = BCrypt.Net.BCrypt.HashPassword("admin123")
                    },
                    new User
                    {
                        Id = Guid.NewGuid(),
                        Username = "user",
                        HashedPassword = BCrypt.Net.BCrypt.HashPassword("user123")
                    }
                };

                context.Users.AddRange(users);
                context.SaveChanges();
            }
        }
    }
}