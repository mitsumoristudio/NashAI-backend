using Microsoft.EntityFrameworkCore;
using Project_Manassas.Model;

namespace Project_Manassas.Database;

public class ProjectContext(DbContextOptions<ProjectContext> options): DbContext(options)
{ 
    public DbSet<ProjectEntity> Projects => Set<ProjectEntity>();
    public DbSet<UserEntity> Users => Set<UserEntity>();
    
    public DbSet<EquipmentEntity> Equipments => Set<EquipmentEntity>();
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<ProjectEntity>(entity =>
        {
            entity.ToTable("Project");
            entity.HasKey(e => e.Id);
        });

        modelBuilder.Entity<UserEntity>(entity =>
        {
            entity.ToTable("User");
            entity.HasKey(e => e.Id);
        });

        modelBuilder.Entity<EquipmentEntity>(entity =>
        {
            entity.ToTable("Equipment");
            entity.HasKey(e => e.Id);
        });
        
        modelBuilder.Entity<ProjectEntity>()
            .HasOne(p => p.User)
            .WithMany(u => u.Projects)
            .HasForeignKey(p => p.UserId)
            .IsRequired(false);
    }
    
}