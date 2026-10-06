using Microsoft.EntityFrameworkCore;
using IPVault.API.Models;

namespace IPVault.API.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Client> Clients => Set<Client>();
    public DbSet<Inventor> Inventors => Set<Inventor>();
    public DbSet<PatentStatus> PatentStatuses => Set<PatentStatus>();
    public DbSet<Patent> Patents => Set<Patent>();
    public DbSet<PatentInventor> PatentInventors => Set<PatentInventor>();
    public DbSet<PatentDocument> PatentDocuments => Set<PatentDocument>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // User Configuration
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(u => u.UserId);
            entity.HasIndex(u => u.Email).IsUnique();
            entity.Property(u => u.Email).HasMaxLength(150).IsRequired();
            entity.Property(u => u.FullName).HasMaxLength(150).IsRequired();
            entity.Property(u => u.Role).HasMaxLength(50).IsRequired();
        });

        // Client Configuration
        modelBuilder.Entity<Client>(entity =>
        {
            entity.HasKey(c => c.ClientId);
            entity.Property(c => c.ClientName).HasMaxLength(200).IsRequired();
            entity.Property(c => c.Email).HasMaxLength(150).IsRequired();
            entity.Property(c => c.Phone).HasMaxLength(50);
            entity.Property(c => c.Company).HasMaxLength(200);
        });

        // Inventor Configuration
        modelBuilder.Entity<Inventor>(entity =>
        {
            entity.HasKey(i => i.InventorId);
            entity.Property(i => i.FirstName).HasMaxLength(100).IsRequired();
            entity.Property(i => i.LastName).HasMaxLength(100).IsRequired();
            entity.Property(i => i.Email).HasMaxLength(150).IsRequired();
            entity.Property(i => i.Phone).HasMaxLength(50);
            entity.Property(i => i.Organization).HasMaxLength(200);
        });

        // PatentStatus Configuration
        modelBuilder.Entity<PatentStatus>(entity =>
        {
            entity.HasKey(s => s.StatusId);
            entity.Property(s => s.StatusName).HasMaxLength(100).IsRequired();
            entity.HasIndex(s => s.StatusName).IsUnique();
        });

        // Patent Configuration
        modelBuilder.Entity<Patent>(entity =>
        {
            entity.HasKey(p => p.PatentId);
            entity.HasIndex(p => p.PatentNumber).IsUnique();
            entity.Property(p => p.PatentNumber).HasMaxLength(100).IsRequired();
            entity.Property(p => p.Title).HasMaxLength(300).IsRequired();
            entity.Property(p => p.Abstract).HasMaxLength(4000);

            // 1-N Relationship: Client -> Patent
            entity.HasOne(p => p.Client)
                .WithMany(c => c.Patents)
                .HasForeignKey(p => p.ClientId)
                .OnDelete(DeleteBehavior.Restrict);

            // 1-N Relationship: PatentStatus -> Patent
            entity.HasOne(p => p.Status)
                .WithMany(s => s.Patents)
                .HasForeignKey(p => p.StatusId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // PatentInventor Junction Table Configuration (Composite Key)
        modelBuilder.Entity<PatentInventor>(entity =>
        {
            entity.HasKey(pi => new { pi.PatentId, pi.InventorId });

            entity.HasOne(pi => pi.Patent)
                .WithMany(p => p.PatentInventors)
                .HasForeignKey(pi => pi.PatentId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(pi => pi.Inventor)
                .WithMany(i => i.PatentInventors)
                .HasForeignKey(pi => pi.InventorId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // PatentDocument Configuration
        modelBuilder.Entity<PatentDocument>(entity =>
        {
            entity.HasKey(d => d.DocumentId);
            entity.Property(d => d.DocumentName).HasMaxLength(255).IsRequired();
            entity.Property(d => d.DocumentType).HasMaxLength(100).IsRequired();
            entity.Property(d => d.FilePath).HasMaxLength(500).IsRequired();

            entity.HasOne(d => d.Patent)
                .WithMany(p => p.Documents)
                .HasForeignKey(d => d.PatentId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
