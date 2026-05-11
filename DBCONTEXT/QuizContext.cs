using Microsoft.EntityFrameworkCore;
using Quiz_Application.Models;
using System.Text.Json;

namespace Quiz_Application.DBCONTEXT
{
    public class QuizContext : DbContext
    {
        public QuizContext(DbContextOptions<QuizContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Quiz> Quizzes { get; set; }
        public DbSet<Question> Questions { get; set; }
        public DbSet<Answer> Answers { get; set; }
        public DbSet<Course> Courses { get; set; }
        public DbSet<Language> Languages { get; set; }
        public DbSet<Result> Results { get; set; }
        public DbSet<Suggestion> Suggestions { get; set; }
        public DbSet<UserResponse> UserResponses { get; set; }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var entries = ChangeTracker
                .Entries()
                .Where(e => e.State == EntityState.Added);

            foreach (var entityEntry in entries)
            {
                var property = entityEntry.Entity.GetType().GetProperty("CreatedDate")
                               ?? entityEntry.Entity.GetType().GetProperty("SavedAt");

                if (property != null && property.PropertyType == typeof(DateTime))
                {
                    var currentVal = (DateTime)property.GetValue(entityEntry.Entity)!;
                    if (currentVal == default)
                    {
                        property.SetValue(entityEntry.Entity, DateTime.UtcNow);
                    }
                }
            }

            return base.SaveChangesAsync(cancellationToken);
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<User>(entity => {
                entity.HasIndex(u => u.Email).IsUnique();

                entity.HasMany(u => u.Quizzes)
                      .WithOne(q => q.User)
                      .HasForeignKey(q => q.UserId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(u => u.Course)
                      .WithMany()
                      .HasForeignKey("CourseId") 
                      .OnDelete(DeleteBehavior.SetNull);
            });
            builder.Entity<Course>()
                .HasMany(c => c.Languages)
                .WithOne(l => l.Course)
                .HasForeignKey(l => l.CourseId);

            builder.Entity<Language>()
                .HasMany(l => l.Quizzes)
                .WithOne(q => q.Language)
                .HasForeignKey(q => q.LanguageId);

            builder.Entity<Quiz>()
                .HasMany(q => q.Questions)
                .WithOne(q => q.Quiz)
                .HasForeignKey(q => q.QuizId);

            builder.Entity<Question>()
                .HasMany(q => q.Answers)
                .WithOne(a => a.Question)
                .HasForeignKey(a => a.QuestionId);

            builder.Entity<Result>()
                .HasOne(r => r.Quiz)
                .WithOne(q => q.Result)
                .HasForeignKey<Result>(r => r.QuizId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Suggestion>(entity =>
            {
                entity.HasKey(s => s.Id);

                entity.HasOne(s => s.User)
                      .WithMany()
                      .HasForeignKey(s => s.UserId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(s => s.Result)
                      .WithMany()
                      .HasForeignKey(s => s.ResultId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(s => s.Language)
                      .WithMany()
                      .HasForeignKey(s => s.LanguageId)
                      .OnDelete(DeleteBehavior.NoAction); 

                entity.HasOne(s => s.Course)
                      .WithMany()
                      .HasForeignKey(s => s.CourseId)
                      .OnDelete(DeleteBehavior.NoAction); 
            });
        }
    }
}
