using Microsoft.EntityFrameworkCore;
using OrchestratorApp.Domain.Entities;

namespace OrchestratorApp.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<ProcedureTemplate> ProcedureTemplates { get; set; } = null!;
        public DbSet<ProcedureStageTemplate> ProcedureStageTemplates { get; set; } = null!;
        public DbSet<ContestTemplate> ContestTemplates { get; set; } = null!;
        public DbSet<StageTemplate> StageTemplates { get; set; } = null!;
        public DbSet<ProcedureInstance> ProcedureInstances { get; set; } = null!;
        public DbSet<ContestInstance> ContestInstances { get; set; } = null!;
        public DbSet<StageConfiguration> StageConfigurations { get; set; } = null!;
        public DbSet<ApplicationInstance> ApplicationInstances { get; set; } = null!;
        public DbSet<StageResult> StageResults { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ProcedureTemplate
            modelBuilder.Entity<ProcedureTemplate>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(255);
                entity.Property(e => e.Version).IsRequired();
                entity.Property(e => e.IsPublished).IsRequired();
                entity.Property(e => e.CreatedAt).IsRequired();
                entity.Property(e => e.UpdatedAt).IsRequired();
                
                // Ensure only one published version at a time for a given name
                entity.HasIndex(e => new { e.Name, e.IsPublished })
                    .HasFilter("\"IsPublished\" = true")
                    .IsUnique();
            });

            // ProcedureStageTemplate
            modelBuilder.Entity<ProcedureStageTemplate>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.StageType).IsRequired();
                entity.Property(e => e.Order).IsRequired();
                entity.Property(e => e.DefaultServiceName).HasMaxLength(255);
                
                entity.HasOne(e => e.ProcedureTemplate)
                    .WithMany(p => p.ProcedureStages)
                    .HasForeignKey(e => e.ProcedureTemplateId)
                    .OnDelete(DeleteBehavior.Cascade);
                    
                entity.HasOne(e => e.PreviousStage)
                    .WithOne(p => p.NextStage)
                    .HasForeignKey<ProcedureStageTemplate>(e => e.PreviousStageId)
                    .OnDelete(DeleteBehavior.ClientSetNull);
            });

            // ContestTemplate
            modelBuilder.Entity<ContestTemplate>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(255);
                entity.Property(e => e.Version).IsRequired();
                entity.Property(e => e.IsPublished).IsRequired();
                
                // Настройка JSON типа для StatusModelArray
                entity.Property(e => e.StatusModelArray)
                      .HasColumnType("jsonb")
                      .HasDefaultValueSql("'[]'::jsonb");
                
                entity.HasOne(e => e.ProcedureTemplate)
                    .WithMany(p => p.ContestTemplates)
                    .HasForeignKey(e => e.ProcedureTemplateId)
                    .OnDelete(DeleteBehavior.Cascade);
                    
                // Ensure only one published version at a time for a given name
                entity.HasIndex(e => new { e.Name, e.IsPublished })
                    .HasFilter("\"IsPublished\" = true")
                    .IsUnique();
            });

            // StageTemplate
            modelBuilder.Entity<StageTemplate>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.StageType).IsRequired();
                entity.Property(e => e.Order).IsRequired();
                entity.Property(e => e.DefaultServiceName).IsRequired().HasMaxLength(255);
                
                entity.HasOne(e => e.ContestTemplate)
                    .WithMany(c => c.Stages)
                    .HasForeignKey(e => e.ContestTemplateId)
                    .OnDelete(DeleteBehavior.Cascade);
                    
                entity.HasOne(e => e.PreviousStage)
                    .WithOne(s => s.NextStage)
                    .HasForeignKey<StageTemplate>(e => e.PreviousStageId)
                    .OnDelete(DeleteBehavior.ClientSetNull);
            });

            // ProcedureInstance
            modelBuilder.Entity<ProcedureInstance>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(255);
                entity.Property(e => e.TemplateVersion).IsRequired();
                entity.Property(e => e.Status).IsRequired().HasMaxLength(50);
                entity.Property(e => e.CreatedAt).IsRequired();
                entity.Property(e => e.UpdatedAt).IsRequired();
                
                entity.HasOne(e => e.ProcedureTemplate)
                    .WithMany(p => p.ProcedureInstances)
                    .HasForeignKey(e => e.ProcedureTemplateId)
                    .OnDelete(DeleteBehavior.Restrict);
                    
                entity.HasOne(e => e.CurrentStage)
                    .WithMany()
                    .HasForeignKey(e => e.CurrentStageId)
                    .OnDelete(DeleteBehavior.ClientSetNull);
            });

            // ContestInstance
            modelBuilder.Entity<ContestInstance>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(255);
                entity.Property(e => e.ContestTemplateVersion).IsRequired();
                entity.Property(e => e.Status).IsRequired().HasMaxLength(50);
                
                entity.HasOne(e => e.ProcedureInstance)
                    .WithMany(p => p.ContestInstances)
                    .HasForeignKey(e => e.ProcedureInstanceId)
                    .OnDelete(DeleteBehavior.Cascade);
                    
                entity.HasOne(e => e.ContestTemplate)
                    .WithMany(c => c.ContestInstances)
                    .HasForeignKey(e => e.ContestTemplateId)
                    .OnDelete(DeleteBehavior.Restrict);
                    
                entity.HasOne(e => e.CurrentStage)
                    .WithMany()
                    .HasForeignKey(e => e.CurrentStageId)
                    .OnDelete(DeleteBehavior.ClientSetNull);
            });

            // StageConfiguration
            modelBuilder.Entity<StageConfiguration>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.ServiceName).IsRequired().HasMaxLength(255);
                
                entity.HasOne(e => e.ContestInstance)
                    .WithMany(c => c.StageConfigurations)
                    .HasForeignKey(e => e.ContestInstanceId)
                    .OnDelete(DeleteBehavior.Cascade);
                    
                entity.HasOne(e => e.StageTemplate)
                    .WithMany(s => s.StageConfigurations)
                    .HasForeignKey(e => e.StageTemplateId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // ApplicationInstance
            modelBuilder.Entity<ApplicationInstance>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Status).IsRequired().HasMaxLength(50);
                entity.Property(e => e.CreatedAt).IsRequired();
                entity.Property(e => e.UpdatedAt).IsRequired();
                
                entity.HasOne(e => e.ContestInstance)
                    .WithMany(c => c.ApplicationInstances)
                    .HasForeignKey(e => e.ContestInstanceId)
                    .OnDelete(DeleteBehavior.Cascade);
                    
                entity.HasOne(e => e.CurrentStage)
                    .WithMany()
                    .HasForeignKey(e => e.CurrentStageId)
                    .OnDelete(DeleteBehavior.ClientSetNull);
            });

            // StageResult
            modelBuilder.Entity<StageResult>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.ResultStatus).IsRequired();
                entity.Property(e => e.CompletedAt).IsRequired();
                entity.Property(e => e.IntegrationEventId).IsRequired();
                
                entity.HasOne(e => e.ApplicationInstance)
                    .WithMany(a => a.StageResults)
                    .HasForeignKey(e => e.ApplicationInstanceId)
                    .OnDelete(DeleteBehavior.Cascade);
                    
                entity.HasOne(e => e.StageTemplate)
                    .WithMany(s => s.StageResults)
                    .HasForeignKey(e => e.StageTemplateId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}
