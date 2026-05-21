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
    /// Configures the entity model.
    /// </summary>
    /// <param name="modelBuilder">The model builder.</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure User entity
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("Users");
            entity.Property(e => e.Description).HasColumnName("Description");
            entity.Property(e => e.Admin).HasColumnName("Admin");
            entity.Property(e => e.LastLogin).HasColumnName("LastLogin");
        });

        // Configure Identity tables
        modelBuilder.Entity<Role>().ToTable("AspNetRoles");
        modelBuilder.Entity<IdentityUserRole<int>>().ToTable("AspNetUserRoles");
        modelBuilder.Entity<IdentityUserClaim<int>>().ToTable("AspNetUserClaims");
        modelBuilder.Entity<IdentityUserLogin<int>>().ToTable("AspNetUserLogins");
        modelBuilder.Entity<IdentityUserToken<int>>().ToTable("AspNetUserTokens");
        modelBuilder.Entity<IdentityRoleClaim<int>>().ToTable("AspNetRoleClaims");

        // Configure Configuration entity
        modelBuilder.Entity<Configuration>(entity =>
        {
            entity.ToTable("Configurations");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id)
                .HasColumnName("ID")
                .ValueGeneratedOnAdd(); // EF gestirà, ma non creerà IDENTITY
            entity.Property(e => e.Key).IsRequired().HasMaxLength(100).HasColumnName("Key");
            entity.Property(e => e.Description).HasMaxLength(500).HasColumnName("Description");
            entity.Property(e => e.Pos).HasColumnName("Pos");
            entity.Property(e => e.Value).HasColumnName("Value");
            entity.HasIndex(e => e.Key).IsUnique();
        });

        // Configure Log entity
        modelBuilder.Entity<Log>(entity =>
        {
            entity.ToTable("Logs");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id)
                .HasColumnName("ID")
                .ValueGeneratedOnAdd();
            entity.Property(e => e.DateTime).IsRequired().HasColumnName("DateTime");
            entity.Property(e => e.Margine).HasColumnType("decimal(18,2)").HasColumnName("Margine");
            entity.Property(e => e.Notes).HasColumnName("Notes");
            entity.Property(e => e.Json).HasColumnName("Json");
            entity.HasIndex(e => e.DateTime);
        });

        // Configure Device entity (mapped to PC table)
        modelBuilder.Entity<Device>(entity =>
        {
            entity.ToTable("PC");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasMaxLength(100).HasColumnName("PC");
            entity.Property(e => e.Title).HasMaxLength(255).HasColumnName("Title");
            entity.Property(e => e.Stato).HasColumnName("STATO");
            entity.Property(e => e.LastUpdate).HasColumnName("LAST_UPDATE");
        });

        // Configure Value entity (telemetry)
        modelBuilder.Entity<Values>(entity =>
        {
            entity.ToTable("Values");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id)
                .HasColumnName("ID")
                .ValueGeneratedOnAdd();
            entity.Property(e => e.Key).HasColumnName("Key");
            entity.Property(e => e.Description).HasMaxLength(255).HasColumnName("Description");
            entity.Property(e => e.Value).HasColumnName("Value");
            entity.Property(e => e.IdUser).HasColumnName("Id_User");
            entity.Property(e => e.DateTime).HasColumnName("DateTime");
            entity.Property(e => e.Account).HasMaxLength(100).HasColumnName("ACCOUNT");
            entity.Property(e => e.Tavolo).HasColumnName("TAVOLO");
            entity.Property(e => e.Mazzo).HasColumnName("MAZZO");
            entity.Property(e => e.Margine).HasColumnType("decimal(18,2)").HasColumnName("MARGINE");
            entity.Property(e => e.MediaOra).HasColumnType("decimal(18,2)").HasColumnName("MEDIA_ORA");
            entity.Property(e => e.Stato).HasMaxLength(50).HasColumnName("STATO");
            entity.Property(e => e.Colore).HasMaxLength(50).HasColumnName("COLORE");
            entity.Property(e => e.ColpoMartingala).HasColumnName("COLPO_MARTINGALA");
            entity.Property(e => e.Valutazione).HasColumnName("VALUTAZIONE");
            entity.Property(e => e.Reason).HasColumnName("REASON");
            entity.Property(e => e.Prediction).HasMaxLength(100).HasColumnName("PREDICTION");
            entity.Property(e => e.Pbt).HasMaxLength(1).HasColumnName("PBT");
            entity.Property(e => e.Tempo).HasMaxLength(10).HasColumnName("TEMPO");

            entity.HasIndex(e => e.Key);
            entity.HasIndex(e => e.DateTime);
            entity.HasIndex(e => new { e.Account, e.Tavolo });

            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.IdUser)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Configure Command entity
        modelBuilder.Entity<Command>(entity =>
        {
            entity.ToTable("Commands");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id)
                .HasColumnName("ID")
                .ValueGeneratedOnAdd();
            entity.Property(e => e.IdCommand).HasColumnName("ID_Command");
            entity.Property(e => e.Pc).HasMaxLength(100).HasColumnName("PC");
            entity.Property(e => e.IdUser).HasColumnName("ID_User");
            entity.Property(e => e.DateTime).HasColumnName("DateTime");

            entity.HasIndex(e => e.DateTime);
            entity.HasIndex(e => e.Pc);

            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.IdUser)
                .OnDelete(DeleteBehavior.Restrict);
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