using Microsoft.EntityFrameworkCore;
using VotingApp.Models;

namespace VotingApp.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<PersonalId> PersonalIds => Set<PersonalId>();
    public DbSet<Poll> Polls => Set<Poll>();
    public DbSet<PollOption> PollOptions => Set<PollOption>();
    public DbSet<Vote> Votes => Set<Vote>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // User
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.Email).HasMaxLength(255);
            entity.Property(e => e.FullName).HasMaxLength(255);
            entity.Property(e => e.PasswordHash).HasMaxLength(255);
        });

        // PersonalId
        modelBuilder.Entity<PersonalId>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Number).IsUnique();
            entity.Property(e => e.Number).HasMaxLength(20);
        });

        // Poll
        modelBuilder.Entity<Poll>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).HasMaxLength(500);
            entity.Property(e => e.Description).HasMaxLength(2000);
            entity.HasOne(e => e.CreatedBy)
                  .WithMany(u => u.CreatedPolls)
                  .HasForeignKey(e => e.CreatedById)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // PollOption
        modelBuilder.Entity<PollOption>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Text).HasMaxLength(500);
            entity.HasOne(e => e.Poll)
                  .WithMany(p => p.Options)
                  .HasForeignKey(e => e.PollId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Vote
        modelBuilder.Entity<Vote>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.User)
                  .WithMany(u => u.Votes)
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Poll)
                  .WithMany(p => p.Votes)
                  .HasForeignKey(e => e.PollId)
                  .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Option)
                  .WithMany(o => o.Votes)
                  .HasForeignKey(e => e.OptionId)
                  .OnDelete(DeleteBehavior.Restrict);

            // Защита от повторного голосования
            entity.HasIndex(e => new { e.UserId, e.PollId }).IsUnique();
        });

        // RefreshToken
        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Token).IsUnique();
            entity.Property(e => e.Token).HasMaxLength(255);
            entity.HasOne(e => e.User)
                  .WithMany(u => u.RefreshTokens)
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // AuditLog
        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.CreatedAt);
            entity.HasIndex(e => e.UserId);
            entity.Property(e => e.Action).HasMaxLength(50);
            entity.Property(e => e.EntityType).HasMaxLength(50);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.UserEmail).HasMaxLength(255);
        });
    }
}
