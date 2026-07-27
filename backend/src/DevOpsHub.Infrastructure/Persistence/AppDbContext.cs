using DevOpsHub.Domain.Users;
using DevOpsHub.Domain.Workspaces;
using DevOpsHub.Domain.Projects;
using DevOpsHub.Domain.Repositories;
using DevOpsHub.Domain.Pipelines;
using DevOpsHub.Domain.Incidents;
using DevOpsHub.Domain.Notifications;
using DevOpsHub.Domain.Observability;
using DevOpsHub.Domain.Documentation;
using DevOpsHub.Domain.Administration;
using Microsoft.EntityFrameworkCore;

namespace DevOpsHub.Infrastructure.Persistence;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<AppUser> Users => Set<AppUser>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<Workspace> Workspaces => Set<Workspace>();
    public DbSet<WorkspaceMember> WorkspaceMembers => Set<WorkspaceMember>();
    public DbSet<WorkspaceInvitation> WorkspaceInvitations => Set<WorkspaceInvitation>();

    
    public DbSet<Project> Projects => Set<Project>();
    public DbSet<Sprint> Sprints => Set<Sprint>();
    public DbSet<Epic> Epics => Set<Epic>();
    public DbSet<WorkItem> WorkItems => Set<WorkItem>();
    public DbSet<Repository> Repositories => Set<Repository>();
    public DbSet<Branch> Branches => Set<Branch>();
    public DbSet<Commit> Commits => Set<Commit>();
    public DbSet<PullRequest> PullRequests => Set<PullRequest>();
    public DbSet<PipelineDefinition> PipelineDefinitions => Set<PipelineDefinition>();
    public DbSet<PipelineRun> PipelineRuns => Set<PipelineRun>();
    public DbSet<PipelineStage> PipelineStages => Set<PipelineStage>();
    public DbSet<Deployment> Deployments => Set<Deployment>();
    public DbSet<MonitoredService> MonitoredServices => Set<MonitoredService>();
    public DbSet<Incident> Incidents => Set<Incident>();
    public DbSet<IncidentUpdate> IncidentUpdates => Set<IncidentUpdate>();
    public DbSet<PostmortemAction> PostmortemActions => Set<PostmortemAction>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<SystemLog> SystemLogs => Set<SystemLog>();
    public DbSet<AuditEntry> AuditEntries => Set<AuditEntry>();
    public DbSet<WikiSpace> WikiSpaces => Set<WikiSpace>();
    public DbSet<WikiDocument> WikiDocuments => Set<WikiDocument>();
    public DbSet<WikiRevision> WikiRevisions => Set<WikiRevision>();
    public DbSet<FeatureFlag> FeatureFlags => Set<FeatureFlag>();
    public DbSet<SystemSetting> SystemSettings => Set<SystemSetting>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AppUser>(entity =>
        {
            entity.ToTable("Users");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Email).HasMaxLength(160).IsRequired();
            entity.HasIndex(x => x.Email).IsUnique();
            entity.Property(x => x.DisplayName).HasMaxLength(80).IsRequired();
            entity.Property(x => x.PasswordHash).HasMaxLength(500).IsRequired();
            entity.Property(x => x.Role).HasMaxLength(30).IsRequired();
            entity.HasMany(x => x.RefreshTokens).WithOne(x => x.User).HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.ToTable("RefreshTokens");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.TokenHash).HasMaxLength(128).IsRequired();
            entity.HasIndex(x => x.TokenHash).IsUnique();
            entity.Property(x => x.ReplacedByTokenHash).HasMaxLength(128);
        });

        modelBuilder.Entity<Workspace>(entity =>
        {
            entity.ToTable("Workspaces");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Name).HasMaxLength(100).IsRequired();
            entity.Property(x => x.Slug).HasMaxLength(80).IsRequired();
            entity.HasIndex(x => x.Slug).IsUnique();
            entity.Property(x => x.Description).HasMaxLength(500);
            entity.HasOne(x => x.Owner).WithMany().HasForeignKey(x => x.OwnerId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<WorkspaceMember>(entity =>
        {
            entity.ToTable("WorkspaceMembers");
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => new { x.WorkspaceId, x.UserId }).IsUnique();
            entity.Property(x => x.Role).HasConversion<string>().HasMaxLength(20);
            entity.HasOne(x => x.Workspace).WithMany(x => x.Members).HasForeignKey(x => x.WorkspaceId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<WorkspaceInvitation>(entity =>
        {
            entity.ToTable("WorkspaceInvitations");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Email).HasMaxLength(160).IsRequired();
            entity.Property(x => x.Token).HasMaxLength(128).IsRequired();
            entity.HasIndex(x => x.Token).IsUnique();
            entity.Property(x => x.Role).HasConversion<string>().HasMaxLength(20);
            entity.HasOne(x => x.Workspace).WithMany(x => x.Invitations).HasForeignKey(x => x.WorkspaceId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Project>(entity =>
        {
            entity.ToTable("Projects");
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => new { x.WorkspaceId, x.Key }).IsUnique();
            entity.Property(x => x.Name).HasMaxLength(120).IsRequired();
            entity.Property(x => x.Key).HasMaxLength(8).IsRequired();
            entity.HasOne(x => x.Workspace).WithMany().HasForeignKey(x => x.WorkspaceId).OnDelete(DeleteBehavior.Cascade);
        });
        modelBuilder.Entity<Sprint>(entity =>
        {
            entity.ToTable("Sprints"); entity.HasKey(x => x.Id); entity.Property(x => x.Name).HasMaxLength(100).IsRequired();
            entity.HasOne(x => x.Project).WithMany(x => x.Sprints).HasForeignKey(x => x.ProjectId).OnDelete(DeleteBehavior.Cascade);
        });
        modelBuilder.Entity<Epic>(entity =>
        {
            entity.ToTable("Epics"); entity.HasKey(x => x.Id); entity.Property(x => x.Title).HasMaxLength(180).IsRequired();
            entity.HasOne(x => x.Project).WithMany(x => x.Epics).HasForeignKey(x => x.ProjectId).OnDelete(DeleteBehavior.Cascade);
        });
        modelBuilder.Entity<WorkItem>(entity =>
        {
            entity.ToTable("WorkItems"); entity.HasKey(x => x.Id); entity.HasIndex(x => new { x.ProjectId, x.Number }).IsUnique(); entity.Property(x => x.Title).HasMaxLength(180).IsRequired();
            entity.HasOne(x => x.Project).WithMany(x => x.WorkItems).HasForeignKey(x => x.ProjectId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.Sprint).WithMany(x => x.WorkItems).HasForeignKey(x => x.SprintId).OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(x => x.Epic).WithMany(x => x.WorkItems).HasForeignKey(x => x.EpicId).OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(x => x.Parent).WithMany(x => x.SubTasks).HasForeignKey(x => x.ParentId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Repository>(entity =>
        {
            entity.ToTable("Repositories"); entity.HasKey(x=>x.Id); entity.HasIndex(x=>new{x.WorkspaceId,x.Name}).IsUnique(); entity.Property(x=>x.Name).HasMaxLength(120).IsRequired(); entity.Property(x=>x.DefaultBranch).HasMaxLength(100).IsRequired();
            entity.HasOne<DevOpsHub.Domain.Workspaces.Workspace>().WithMany().HasForeignKey(x=>x.WorkspaceId).OnDelete(DeleteBehavior.Cascade);
        });
        modelBuilder.Entity<Branch>(entity =>
        { entity.ToTable("Branches"); entity.HasKey(x=>x.Id); entity.HasIndex(x=>new{x.RepositoryId,x.Name}).IsUnique(); entity.Property(x=>x.Name).HasMaxLength(140).IsRequired(); entity.HasOne(x=>x.Repository).WithMany(x=>x.Branches).HasForeignKey(x=>x.RepositoryId).OnDelete(DeleteBehavior.Cascade); });
        modelBuilder.Entity<Commit>(entity =>
        { entity.ToTable("Commits"); entity.HasKey(x=>x.Id); entity.Property(x=>x.Sha).HasMaxLength(40).IsRequired(); entity.Property(x=>x.Message).HasMaxLength(300).IsRequired(); entity.HasOne(x=>x.Repository).WithMany(x=>x.Commits).HasForeignKey(x=>x.RepositoryId).OnDelete(DeleteBehavior.Cascade); });
        modelBuilder.Entity<PullRequest>(entity =>
        { entity.ToTable("PullRequests"); entity.HasKey(x=>x.Id); entity.HasIndex(x=>new{x.RepositoryId,x.Number}).IsUnique(); entity.Property(x=>x.Title).HasMaxLength(220).IsRequired(); entity.Property(x=>x.Status).HasConversion<string>(); entity.Property(x=>x.ReviewState).HasConversion<string>(); entity.HasOne(x=>x.Repository).WithMany(x=>x.PullRequests).HasForeignKey(x=>x.RepositoryId).OnDelete(DeleteBehavior.Cascade); });

        modelBuilder.Entity<PipelineDefinition>(entity =>
        { entity.ToTable("PipelineDefinitions"); entity.HasKey(x=>x.Id); entity.Property(x=>x.Name).HasMaxLength(140).IsRequired(); entity.Property(x=>x.Branch).HasMaxLength(140).IsRequired(); });
        modelBuilder.Entity<PipelineRun>(entity =>
        { entity.ToTable("PipelineRuns"); entity.HasKey(x=>x.Id); entity.HasIndex(x=>new{x.PipelineDefinitionId,x.Number}).IsUnique(); entity.Property(x=>x.Status).HasConversion<string>(); entity.Property(x=>x.Trigger).HasConversion<string>(); entity.Property(x=>x.CommitSha).HasMaxLength(40); entity.HasOne(x=>x.PipelineDefinition).WithMany(x=>x.Runs).HasForeignKey(x=>x.PipelineDefinitionId).OnDelete(DeleteBehavior.Cascade); });
        modelBuilder.Entity<PipelineStage>(entity =>
        { entity.ToTable("PipelineStages"); entity.HasKey(x=>x.Id); entity.Property(x=>x.Name).HasMaxLength(100).IsRequired(); entity.Property(x=>x.Status).HasConversion<string>(); entity.HasOne(x=>x.PipelineRun).WithMany(x=>x.Stages).HasForeignKey(x=>x.PipelineRunId).OnDelete(DeleteBehavior.Cascade); });
        modelBuilder.Entity<Deployment>(entity =>
        { entity.ToTable("Deployments"); entity.HasKey(x=>x.Id); entity.Property(x=>x.Environment).HasConversion<string>(); entity.Property(x=>x.Status).HasConversion<string>(); entity.Property(x=>x.Version).HasMaxLength(50); entity.Property(x=>x.Url).HasMaxLength(300); entity.HasOne(x=>x.PipelineRun).WithMany(x=>x.Deployments).HasForeignKey(x=>x.PipelineRunId).OnDelete(DeleteBehavior.Cascade); });

        modelBuilder.Entity<MonitoredService>(entity =>
        { entity.ToTable("MonitoredServices"); entity.HasKey(x=>x.Id); entity.HasIndex(x=>new{x.WorkspaceId,x.Slug}).IsUnique(); entity.Property(x=>x.Name).HasMaxLength(120).IsRequired(); entity.Property(x=>x.Status).HasConversion<string>(); });
        modelBuilder.Entity<Incident>(entity =>
        { entity.ToTable("Incidents"); entity.HasKey(x=>x.Id); entity.HasIndex(x=>new{x.WorkspaceId,x.Number}).IsUnique(); entity.Property(x=>x.Title).HasMaxLength(220).IsRequired(); entity.Property(x=>x.Severity).HasConversion<string>(); entity.Property(x=>x.Status).HasConversion<string>(); entity.HasOne(x=>x.MonitoredService).WithMany(x=>x.Incidents).HasForeignKey(x=>x.MonitoredServiceId).OnDelete(DeleteBehavior.Cascade); });
        modelBuilder.Entity<IncidentUpdate>(entity =>
        { entity.ToTable("IncidentUpdates"); entity.HasKey(x=>x.Id); entity.Property(x=>x.Status).HasConversion<string>(); entity.Property(x=>x.Message).HasMaxLength(2000).IsRequired(); entity.HasOne(x=>x.Incident).WithMany(x=>x.Updates).HasForeignKey(x=>x.IncidentId).OnDelete(DeleteBehavior.Cascade); });
        modelBuilder.Entity<PostmortemAction>(entity =>
        { entity.ToTable("PostmortemActions"); entity.HasKey(x=>x.Id); entity.Property(x=>x.Title).HasMaxLength(240).IsRequired(); entity.HasOne(x=>x.Incident).WithMany(x=>x.Actions).HasForeignKey(x=>x.IncidentId).OnDelete(DeleteBehavior.Cascade); });

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.ToTable("Notifications"); entity.HasKey(x=>x.Id); entity.HasIndex(x=>new{x.UserId,x.IsRead,x.CreatedAtUtc});
            entity.Property(x=>x.Type).HasConversion<string>().HasMaxLength(30); entity.Property(x=>x.Title).HasMaxLength(180).IsRequired(); entity.Property(x=>x.Message).HasMaxLength(1000).IsRequired(); entity.Property(x=>x.ActionUrl).HasMaxLength(300); entity.Property(x=>x.Source).HasMaxLength(80);
        });

        modelBuilder.Entity<SystemLog>(entity =>
        {
            entity.ToTable("SystemLogs"); entity.HasKey(x=>x.Id); entity.HasIndex(x=>new{x.Level,x.CreatedAtUtc}); entity.HasIndex(x=>x.RequestId);
            entity.Property(x=>x.Level).HasConversion<string>().HasMaxLength(20); entity.Property(x=>x.Message).HasMaxLength(2000).IsRequired(); entity.Property(x=>x.Category).HasMaxLength(120); entity.Property(x=>x.Path).HasMaxLength(500); entity.Property(x=>x.Method).HasMaxLength(12);
        });

        modelBuilder.Entity<WikiSpace>(entity =>
        { entity.ToTable("WikiSpaces"); entity.HasKey(x=>x.Id); entity.HasIndex(x=>new{x.WorkspaceId,x.Slug}).IsUnique(); entity.Property(x=>x.Name).HasMaxLength(120).IsRequired(); entity.Property(x=>x.Slug).HasMaxLength(120).IsRequired(); entity.Property(x=>x.Description).HasMaxLength(500); });
        modelBuilder.Entity<WikiDocument>(entity =>
        { entity.ToTable("WikiDocuments"); entity.HasKey(x=>x.Id); entity.HasIndex(x=>new{x.WikiSpaceId,x.Slug}).IsUnique(); entity.Property(x=>x.Title).HasMaxLength(180).IsRequired(); entity.Property(x=>x.Slug).HasMaxLength(180).IsRequired(); entity.Property(x=>x.Category).HasMaxLength(80).IsRequired(); entity.Property(x=>x.Status).HasConversion<string>().HasMaxLength(20); entity.HasOne(x=>x.WikiSpace).WithMany(x=>x.Documents).HasForeignKey(x=>x.WikiSpaceId).OnDelete(DeleteBehavior.Cascade); });
        modelBuilder.Entity<WikiRevision>(entity =>
        { entity.ToTable("WikiRevisions"); entity.HasKey(x=>x.Id); entity.Property(x=>x.Title).HasMaxLength(180).IsRequired(); entity.Property(x=>x.Category).HasMaxLength(80).IsRequired(); entity.HasOne(x=>x.Document).WithMany(x=>x.Revisions).HasForeignKey(x=>x.DocumentId).OnDelete(DeleteBehavior.Cascade); });

        modelBuilder.Entity<FeatureFlag>(entity =>
        { entity.ToTable("FeatureFlags"); entity.HasKey(x=>x.Id); entity.HasIndex(x=>x.Key).IsUnique(); entity.Property(x=>x.Key).HasMaxLength(100).IsRequired(); entity.Property(x=>x.Description).HasMaxLength(500).IsRequired(); });

        modelBuilder.Entity<SystemSetting>(entity =>
        { entity.ToTable("SystemSettings"); entity.HasKey(x=>x.Id); entity.HasIndex(x=>x.Key).IsUnique(); entity.Property(x=>x.Key).HasMaxLength(120).IsRequired(); entity.Property(x=>x.Value).HasMaxLength(2000).IsRequired(); entity.Property(x=>x.Category).HasMaxLength(80).IsRequired(); });

        modelBuilder.Entity<AuditEntry>(entity =>
        {
            entity.ToTable("AuditEntries"); entity.HasKey(x=>x.Id); entity.HasIndex(x=>x.CreatedAtUtc); entity.HasIndex(x=>x.RequestId);
            entity.Property(x=>x.Action).HasMaxLength(30).IsRequired(); entity.Property(x=>x.EntityType).HasMaxLength(120).IsRequired(); entity.Property(x=>x.EntityId).HasMaxLength(80); entity.Property(x=>x.UserEmail).HasMaxLength(160); entity.Property(x=>x.IpAddress).HasMaxLength(80);
        });
    }
}
