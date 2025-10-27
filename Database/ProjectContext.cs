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
    
    // New vector-related tables
    public DbSet<DocumentEmbeddingVB> DocumentEmbeddings => Set<DocumentEmbeddingVB>();
    
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
            entity.Property(e => e.Embeddings).HasColumnType("vector(1536)")
                ;
        });
        
        // // ChatSessions
        // modelBuilder.Entity<ChatSessionVBModel>(entity =>
        // {
        //     entity.ToTable("ChatSession");
        //     entity.HasKey(e => e.Id);
        //     entity.HasIndex(e => e.SessionId).IsUnique();
        //     
        //     entity.HasOne<ChatSessionVBModel>()
        //         .WithMany()
        //         .HasForeignKey(e => e.SessionId)
        //         .OnDelete(DeleteBehavior.Cascade);
        //     
        // });
        //
        // // ChatMessages
        // modelBuilder.Entity<ChatMessageVBModel>(entity =>
        // {
        //     entity.ToTable("ChatMessage");
        //     entity.HasKey(e => e.Id);
        //
        //     entity.HasOne<ChatSessionVBModel>()
        //         .WithMany(s => s.Messages)
        //         .HasForeignKey(e => e.SessionId);
        //
        //     entity.Property(e => e.MessageContent).IsRequired();
        //
        //     // Convert enum to string for PostgreSQL
        //     entity.Property(e => e.Role)
        //         .HasConversion(new EnumToStringConverter<ChatRole>())
        //         .IsRequired();
        // });
        //
        // // Chat Sources (pgvector)
        // modelBuilder.Entity<ChatSourceDBModel>(entity =>
        // {
        //     entity.ToTable("ChatSource");
        //     entity.HasKey(e => e.Id);
        //
        //     entity.HasOne<ChatMessageVBModel>()
        //         .WithMany(s => s.Sources)
        //         .HasForeignKey(e => e.Id);
        //
        //     entity.Property(e => e.Snippet).IsRequired();
        //
        //     entity.Property(e => e.Snippet).HasColumnType("text");
        //
        //     // Use PostgresSQL vector types for embeddings
        //     entity.Property(e => e.Embeddings)
        //         .HasColumnType("vector(1536)");
        //
        // });
        
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