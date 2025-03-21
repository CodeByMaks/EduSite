using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using EduSite.Models;

namespace EduSite.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Course> Courses { get; set; }
        public DbSet<CourseModule> CourseModules { get; set; }
        public DbSet<ModuleContent> ModuleContents { get; set; }
        public DbSet<CompletedContent> CompletedContents { get; set; }
        public DbSet<UserCourse> UserCourses { get; set; }
        public DbSet<Book> Books { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<UserCourse>()
                .HasKey(uc => new { uc.UserId, uc.CourseId });

            builder.Entity<UserCourse>()
                .HasOne(uc => uc.User)
                .WithMany(u => u.EnrolledCourses)
                .HasForeignKey(uc => uc.UserId);

            builder.Entity<UserCourse>()
                .HasOne(uc => uc.Course)
                .WithMany(c => c.EnrolledUsers)
                .HasForeignKey(uc => uc.CourseId);

            builder.Entity<CompletedContent>()
                .HasOne(cc => cc.User)
                .WithMany()
                .HasForeignKey(cc => cc.UserId);

            builder.Entity<CompletedContent>()
                .HasOne(cc => cc.Content)
                .WithMany(mc => mc.CompletedByUsers)
                .HasForeignKey(cc => cc.ContentId);

            builder.Entity<CourseModule>()
                .HasOne(cm => cm.Course)
                .WithMany(c => c.Modules)
                .HasForeignKey(cm => cm.CourseId);

            builder.Entity<ModuleContent>()
                .HasOne(mc => mc.Module)
                .WithMany(m => m.Contents)
                .HasForeignKey(mc => mc.ModuleId);
        }
    }
}
