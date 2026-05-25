using Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace WebApi.Data;

/// <summary>
/// Application database context for Entity Framework Core with Identity.
/// </summary>
public class AppDbContext : IdentityDbContext<User, Role, int>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AppDbContext"/> class.
    /// </summary>
    /// <param name="options">The database context options.</param>
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    /// <summary>
    /// Gets or sets the Configurations DbSet.
    /// </summary>
    public DbSet<Configuration> Configurations { get; set; }

    /// <summary>
    /// Gets or sets the Logs DbSet.
    /// </summary>
    public DbSet<Log> Logs { get; set; }

    /// <summary>
    /// Gets or sets the Devices/PCs DbSet.
    /// </summary>
    public DbSet<Device> Devices { get; set; }

    /// <summary>
    /// Gets or sets the Values DbSet (telemetry data).
    /// </summary>
    public DbSet<Values> Values { get; set; }

    /// <summary>
    /// Gets or sets the Commands DbSet.
    /// </summary>
    public DbSet<Command> Commands { get; set; }

    /// <summary>
    /// Gets or sets the UserGridConfigurations DbSet.
    /// </summary>
    public DbSet<UserGridConfiguration> UserGridConfigurations { get; set; }

    /// <summary>
    /// Gets or sets the accounting mission sessions used by financial reports.
    /// </summary>
    public DbSet<MissionSession> MissionSessions { get; set; }

    /// <summary>
    /// Gets or sets the accounting margin samples for mission sessions.
    /// </summary>
    public DbSet<MissionMarginSample> MissionMarginSamples { get; set; }

    public DbSet<UserNotificationSetting> UserNotificationSettings { get; set; }

    public DbSet<UserAccessEvent> UserAccessEvents { get; set; }

    public DbSet<PcCurrentStatus> PcCurrentStatuses { get; set; }

    /// <summary>
    /// Session statistics written by the Decisore (dbo.Statistiche). Read-only.
    /// </summary>
    public DbSet<Statistica> Statistiche { get; set; }

    /// <summary>
    /// Margin time-series written by the Decisore (dbo.Margini). Read-only.
    /// </summary>
    public DbSet<Margine> Margini { get; set; }

    /// <summary>
    /// Configures the entity model.
    /// </summary>
    /// <param name="modelBuilder">The model builder.</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure User entity
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("Users_v2");
            entity.Property(e => e.Description).HasColumnName("Description");
            entity.Property(e => e.Admin).HasColumnName("Admin");
            entity.Property(e => e.LastLogin).HasColumnName("LastLogin");
        });

        // Configure Identity tables
        modelBuilder.Entity<Role>().ToTable("AspNetRoles");
        modelBuilder.Entity<IdentityUserRole<int>>().ToTable("AspNetUserRoles");
        modelBuilder.Entity<IdentityUserClaim<int>>().ToTable("AspNetUserClaims");
        modelBuilder.Entity<IdentityUserLogin<int>>().ToTable("AspNetUserLogins");
        modelBuilder.Entity<IdentityRoleClaim<int>>().ToTable("AspNetRoleClaims");
        modelBuilder.Ignore<IdentityUserToken<int>>();

        // Configure Configuration entity (legacy: K is PK)
        modelBuilder.Entity<Configuration>(entity =>
        {
            entity.ToTable("Configurations");
            entity.HasKey(e => e.Key);
            entity.Property(e => e.Key).HasColumnName("K").HasMaxLength(50);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.Value).HasMaxLength(4000);
        });

        // Configure ApiLogs entity (production dbo.ApiLogs)
        modelBuilder.Entity<Log>(entity =>
        {
            entity.ToTable("ApiLogs");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id)
                .HasColumnName("ID")
                .ValueGeneratedOnAdd();
            entity.Property(e => e.Description).IsRequired();
            entity.Property(e => e.Category).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Action).IsRequired();
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.HasIndex(e => e.CreatedAt);
            entity.HasIndex(e => e.Category);
        });

        // Configure mission report entities. These are intentionally separate from runtime Logs/Values.
        modelBuilder.Entity<MissionSession>(entity =>
        {
            entity.ToTable("MissionSessions");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id)
                .HasColumnName("ID")
                .ValueGeneratedOnAdd();
            entity.Property(e => e.MissionKey).HasMaxLength(128);
            entity.Property(e => e.TotalMargin).HasColumnType("decimal(18,2)");
            entity.Property(e => e.LastTotalMarginForRealHands).HasColumnType("decimal(18,2)");
            entity.Property(e => e.GlobalTarget).HasColumnType("decimal(18,2)");
            entity.Property(e => e.KFactor).HasColumnType("decimal(18,2)");
            entity.Property(e => e.RuntimeMode).HasMaxLength(32);
            entity.Property(e => e.FinalizationReason).HasMaxLength(128);
            entity.HasIndex(e => e.MissionKey)
                .IsUnique()
                .HasFilter("[MissionKey] IS NOT NULL");
            entity.HasIndex(e => e.RuntimeMode);
            entity.HasIndex(e => e.StartTime);
            entity.HasIndex(e => e.EndTime);
            entity.HasIndex(e => e.Completed);
        });

        modelBuilder.Entity<MissionMarginSample>(entity =>
        {
            entity.ToTable("MissionMarginSamples");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id)
                .HasColumnName("ID")
                .ValueGeneratedOnAdd();
            entity.Property(e => e.TotalMargin).HasColumnType("decimal(18,2)");
            entity.Property(e => e.VmCurrent).HasColumnType("decimal(18,2)");
            entity.Property(e => e.RuntimeMode).HasMaxLength(32);
            entity.HasIndex(e => e.SessionId);
            entity.HasIndex(e => e.Timestamp);
            entity.HasIndex(e => e.RuntimeMode);
            entity.HasOne(e => e.Session)
                .WithMany(e => e.Samples)
                .HasForeignKey(e => e.SessionId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<UserNotificationSetting>(entity =>
        {
            entity.ToTable("UserNotificationSettings");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id)
                .HasColumnName("ID")
                .ValueGeneratedOnAdd();
            entity.Property(e => e.NotificationEmail).HasMaxLength(256);
            entity.HasIndex(e => e.UserId).IsUnique();
            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<UserAccessEvent>(entity =>
        {
            entity.ToTable("UserAccessEvents");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id)
                .HasColumnName("ID")
                .ValueGeneratedOnAdd();
            entity.Property(e => e.Username).HasMaxLength(256);
            entity.Property(e => e.EventType).HasMaxLength(32);
            entity.Property(e => e.IpAddress).HasMaxLength(128);
            entity.Property(e => e.Page).HasMaxLength(512);
            entity.Property(e => e.UserAgent).HasMaxLength(1024);
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.Username);
            entity.HasIndex(e => e.EventType);
            entity.HasIndex(e => e.OccurredAtUtc);
            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // Configure Device entity (production dbo.Pc)
        modelBuilder.Entity<Device>(entity =>
        {
            entity.ToTable("Pc");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.Name).HasMaxLength(50).HasColumnName("NAME");
            entity.Property(e => e.Total).HasColumnType("decimal(19,0)").HasColumnName("TOTAL");
        });

        // Configure Value entity (production legacy Values)
        modelBuilder.Entity<Values>(entity =>
        {
            entity.ToTable("Values");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("ID").HasColumnType("decimal(18,0)");
            entity.Property(e => e.Key).HasColumnName("Key").HasColumnType("decimal(18,0)");
            entity.Property(e => e.Description).HasMaxLength(50);
            entity.Property(e => e.Value).HasMaxLength(50);
            entity.Property(e => e.IdUser).HasColumnName("ID_User");
            entity.Property(e => e.DateTime).HasColumnName("Datetime");

            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.IdUser)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Configure Command entity (production legacy Commands)
        modelBuilder.Entity<Command>(entity =>
        {
            entity.ToTable("Commands");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("ID").HasColumnType("decimal(18,0)");
            entity.Property(e => e.IdCommand).HasColumnName("ID_Command").HasColumnType("decimal(18,0)");
            entity.Property(e => e.Pc).HasMaxLength(50).HasColumnName("PC");
            entity.Property(e => e.IdUser).HasColumnName("ID_User");
            entity.Property(e => e.DateTime).HasColumnName("Datetime");
            entity.Property(e => e.BitSent).HasColumnName("Bit_Sent");

            entity.HasIndex(e => e.DateTime);
            entity.HasIndex(e => e.Pc);

            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.IdUser)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Statistiche — read-only, written by Decisore
        modelBuilder.Entity<Statistica>(entity =>
        {
            entity.ToTable("Statistiche", t => t.ExcludeFromMigrations());
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.MargineTot).HasColumnType("decimal(19,0)");
            entity.Property(e => e.MargineMin).HasColumnType("decimal(19,0)");
            entity.Property(e => e.MargineMax).HasColumnType("decimal(19,0)");
            entity.Property(e => e.Telemetry).HasMaxLength(4000);
        });

        // Margini — read-only, written by Decisore via InsertMargine SP
        modelBuilder.Entity<Margine>(entity =>
        {
            entity.ToTable("Margini", t => t.ExcludeFromMigrations());
            entity.HasKey(e => e.Id);
            entity.Property(e => e.MargineValue).HasColumnName("Margine").HasColumnType("decimal(18,0)");
            entity.Property(e => e.Data).HasColumnName("Data");
        });

        // Configure UserGridConfiguration entity
        modelBuilder.Entity<UserGridConfiguration>(entity =>
        {
            entity.ToTable("User_Grid_Configurations");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id)
                .HasColumnName("ID")
                .ValueGeneratedOnAdd();
            entity.Property(e => e.IdUser).HasColumnName("ID_user");
            entity.Property(e => e.PageName).HasMaxLength(255).HasColumnName("page_name");
            entity.Property(e => e.GridName).HasMaxLength(255).HasColumnName("grid_name");
            entity.Property(e => e.ColumnName).HasMaxLength(255).HasColumnName("column_name");
            entity.Property(e => e.Display).HasColumnName("display");

            entity.HasIndex(e => new { e.IdUser, e.PageName, e.GridName });

            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.IdUser)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}