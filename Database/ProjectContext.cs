using Microsoft.EntityFrameworkCore;
using NashAI_app.Model;
using Project_Manassas.Model;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.Extensions.AI;
using Pgvector;
using Pgvector.EntityFrameworkCore;


namespace Project_Manassas.Database;



public class ProjectContext(DbContextOptions<ProjectContext> options): DbContext(options)
{ 
    
    public DbSet<ProjectEntity> Projects => Set<ProjectEntity>();
    public DbSet<UserEntity> Users => Set<UserEntity>();
    
    public DbSet<EquipmentEntity> Equipments => Set<EquipmentEntity>();
    
    public DbSet<VerificationCode> VerificationCodes => Set<VerificationCode>();
    
    // New vector-related tables
    public DbSet<DocumentEmbeddingVB> DocumentEmbeddings => Set<DocumentEmbeddingVB>();
    
    // Store entire PDF
    public DbSet<PdfFileEntity> PdfFiles => Set<PdfFileEntity>();
    
    // public DbSet<ChatSessionVBModel> ChatSessions => Set<ChatSessionVBModel>();
    //
    // public DbSet<ChatMessageVBModel> ChatMessages => Set<ChatMessageVBModel>();
    //
    //
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var vectorConverter = new ValueConverter<float[], float[]>(
            v => v,   // No conversion needed for Npgsql vector provider
            v => v
        );

        base.OnModelCreating(modelBuilder);

        modelBuilder.HasPostgresExtension("vector");

        // DocumentEmbedding
        modelBuilder.Entity<DocumentEmbeddingVB>(entity =>
        {
            entity.ToTable("document_embedding");
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.DocumentId).IsRequired();
            entity.Property(e => e.Embeddings).HasColumnType("vector(1536)");
            
            // Add unique constraint
            entity.HasIndex(e => new { e.DocumentId, e.PageNumber, e.Content })
                .IsUnique()
                .HasDatabaseName("constraint_1");

        });
        
        modelBuilder.Entity<VerificationCode>(entity =>
        {
            entity.ToTable("VerificationCodes");
            entity.HasKey(e => e.Id);
        });
        
        
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