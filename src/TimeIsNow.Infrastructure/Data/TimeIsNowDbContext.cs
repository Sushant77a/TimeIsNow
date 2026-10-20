using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using TimeIsNow.Application.Interfaces.Security;
using TimeIsNow.Domain.Entities;

namespace TimeIsNow.Infrastructure.Data;

public class TimeIsNowDbContext(
    DbContextOptions<TimeIsNowDbContext> options,
    ICurrentUser currentUser) : DbContext(options)
{
    public int CurrentUserId => currentUser.UserId;

    public DbSet<User> Users => Set<User>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<UserProfile> UserProfiles => Set<UserProfile>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Todo> Todos => Set<Todo>();
    public DbSet<DailyEntry> DailyEntries => Set<DailyEntry>();
    public DbSet<JournalEntry> JournalEntries => Set<JournalEntry>();
    public DbSet<ScheduleItem> ScheduleItems => Set<ScheduleItem>();
    public DbSet<FinanceEntry> FinanceEntries => Set<FinanceEntry>();
    public DbSet<Goal> Goals => Set<Goal>();
    public DbSet<Habit> Habits => Set<Habit>();
    public DbSet<HabitLog> HabitLogs => Set<HabitLog>();
    public DbSet<Tag> Tags => Set<Tag>();
    public DbSet<Note> Notes => Set<Note>();
    public DbSet<NoteTag> NoteTags => Set<NoteTag>();
    public DbSet<TodoTag> TodoTags => Set<TodoTag>();
    public DbSet<JournalTag> JournalTags => Set<JournalTag>();
    public DbSet<CategoryModule> CategoryModules => Set<CategoryModule>();
    public DbSet<GlobalModule> GlobalModules => Set<GlobalModule>();

    public override int SaveChanges()
    {
        StampChanges();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        StampChanges();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void StampChanges()
    {
        var userId = CurrentUserId;

        foreach (var entry in ChangeTracker.Entries<BaseEntity>()
            .Where(e => e.State == EntityState.Modified))
        {
            entry.Entity.UpdatedAt = DateTime.UtcNow;
        }

        if (userId == 0)
            return;

        foreach (var entry in ChangeTracker.Entries<IUserOwned>()
            .Where(e => e.State == EntityState.Added && e.Entity.UserId == 0))
        {
            entry.Entity.UserId = userId;
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(e =>
        {
            e.ToTable("User");
            e.HasKey(x => x.Id);
            e.Property(x => x.Email).HasMaxLength(256).IsRequired();
            e.Property(x => x.PasswordHash).HasMaxLength(256).IsRequired();
            e.Property(x => x.DisplayName).HasMaxLength(100).IsRequired();
            e.HasIndex(x => x.Email).IsUnique();
            e.Property(x => x.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
            e.Property(x => x.UpdatedAt).ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
        });

        modelBuilder.Entity<RefreshToken>(e =>
        {
            e.ToTable("RefreshToken");
            e.HasKey(x => x.Id);
            e.Property(x => x.TokenHash).HasMaxLength(128).IsRequired();
            e.Property(x => x.ReplacedByToken).HasMaxLength(128);
            e.HasIndex(x => x.TokenHash).IsUnique();
            e.HasIndex(x => x.UserId);
            e.Property(x => x.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP(6)");

            e.HasOne(x => x.User).WithMany()
                .HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<UserProfile>(e =>
        {
            e.ToTable("UserProfile");
            e.HasKey(x => x.Id);
            e.Property(x => x.DisplayName).HasMaxLength(100).IsRequired();
            e.Property(x => x.Currency).HasMaxLength(10).IsRequired();
            e.Property(x => x.Theme).HasMaxLength(20).IsRequired();
            e.HasIndex(x => x.UserId).IsUnique();
            e.Property(x => x.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
            e.Property(x => x.UpdatedAt).ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
        });

        modelBuilder.Entity<Category>(e =>
        {
            e.ToTable("Category");
            e.Property(x => x.Name).HasMaxLength(100).IsRequired();
            e.Property(x => x.Description).HasMaxLength(500);
            e.Property(x => x.Color).HasMaxLength(7).IsRequired();
            e.Property(x => x.Icon).HasMaxLength(50);
            e.HasIndex(x => new { x.UserId, x.Name });
            ConfigureBase(e);
        });

        modelBuilder.Entity<Todo>(e =>
        {
            e.ToTable("Todo");
            e.Property(x => x.Title).HasMaxLength(300).IsRequired();
            e.HasIndex(x => new { x.CategoryId, x.IsDone });
            e.HasIndex(x => x.DueDate);
            e.HasIndex(x => x.ParentTodoId);
            ConfigureBase(e);
            ConfigureCategoryRef(e, c => c.Todos);

            e.HasOne(x => x.Parent).WithMany(t => t.SubTasks)
                .HasForeignKey(x => x.ParentTodoId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<DailyEntry>(e =>
        {
            e.ToTable("DailyEntry", t =>
            {
                t.HasCheckConstraint("CK_DailyEntry_Mood_Range", "Mood BETWEEN 1 AND 5");
                t.HasCheckConstraint("CK_DailyEntry_Energy_Range", "Energy BETWEEN 1 AND 5");
            });
            e.Property(x => x.Content).IsRequired();
            e.HasIndex(x => new { x.CategoryId, x.Date });
            e.HasIndex(x => x.Date);
            ConfigureBase(e);
            ConfigureCategoryRef(e, c => c.DailyEntries);
        });

        modelBuilder.Entity<JournalEntry>(e =>
        {
            e.ToTable("JournalEntry", t =>
            {
                t.HasCheckConstraint("CK_JournalEntry_Mood_Range", "Mood BETWEEN 1 AND 5");
                t.HasCheckConstraint("CK_JournalEntry_Energy_Range", "Energy BETWEEN 1 AND 5");
            });
            e.Property(x => x.Title).HasMaxLength(300);
            e.Property(x => x.Content).IsRequired();
            e.HasIndex(x => new { x.CategoryId, x.Date });
            ConfigureBase(e);
            ConfigureCategoryRef(e, c => c.JournalEntries);
        });

        modelBuilder.Entity<ScheduleItem>(e =>
        {
            e.ToTable("ScheduleItem");
            e.Property(x => x.Title).HasMaxLength(300).IsRequired();
            e.HasIndex(x => new { x.CategoryId, x.ScheduledAt });
            e.HasIndex(x => x.ScheduledAt);
            e.HasIndex(x => x.GoalId);
            ConfigureBase(e);
            ConfigureCategoryRef(e, c => c.ScheduleItems);

            e.HasOne(x => x.Goal).WithMany(g => g.ScheduleItems)
                .HasForeignKey(x => x.GoalId).OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<FinanceEntry>(e =>
        {
            e.ToTable("FinanceEntry", t =>
                t.HasCheckConstraint("CK_FinanceEntry_Amount_Positive", "Amount > 0"));
            e.Property(x => x.Amount).HasPrecision(18, 2);
            e.Property(x => x.Description).HasMaxLength(500);
            e.HasIndex(x => new { x.CategoryId, x.Date });
            e.HasIndex(x => x.Date);
            ConfigureBase(e);
            ConfigureCategoryRef(e, c => c.FinanceEntries);
        });

        modelBuilder.Entity<Goal>(e =>
        {
            e.ToTable("Goal", t =>
                t.HasCheckConstraint("CK_Goal_ProgressPercent_Range", "ProgressPercent BETWEEN 0 AND 100"));
            e.Property(x => x.Title).HasMaxLength(300).IsRequired();
            e.HasIndex(x => new { x.CategoryId, x.Status });
            ConfigureBase(e);
            ConfigureCategoryRef(e, c => c.Goals);
        });

        modelBuilder.Entity<Habit>(e =>
        {
            e.ToTable("Habit");
            e.Property(x => x.Title).HasMaxLength(200).IsRequired();
            e.HasIndex(x => new { x.CategoryId, x.IsActive });
            ConfigureBase(e);
            ConfigureCategoryRef(e, c => c.Habits);
        });

        modelBuilder.Entity<HabitLog>(e =>
        {
            e.ToTable("HabitLog");
            e.HasKey(x => x.Id);
            e.Property(x => x.Note).HasMaxLength(500);
            e.HasIndex(x => new { x.HabitId, x.Date }).IsUnique();
            e.HasIndex(x => x.UserId);
            e.Property(x => x.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP(6)");

            e.HasOne(x => x.Habit).WithMany(h => h.Logs)
                .HasForeignKey(x => x.HabitId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Tag>(e =>
        {
            e.ToTable("Tag");
            e.HasKey(x => x.Id);
            e.Property(x => x.Name).HasMaxLength(100).IsRequired();
            e.HasIndex(x => new { x.UserId, x.Name }).IsUnique();
            e.Property(x => x.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
        });

        modelBuilder.Entity<Note>(e =>
        {
            e.ToTable("Note");
            e.Property(x => x.Title).HasMaxLength(300).IsRequired();
            e.HasIndex(x => x.CategoryId);
            e.HasIndex(x => x.IsPinned);
            ConfigureBase(e);
            ConfigureCategoryRef(e, c => c.Notes);
        });

        modelBuilder.Entity<NoteTag>(e =>
        {
            e.ToTable("NoteTag");
            e.HasKey(x => new { x.NoteId, x.TagId });
            e.HasIndex(x => x.TagId);

            e.HasOne(x => x.Note).WithMany(n => n.Tags)
                .HasForeignKey(x => x.NoteId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Tag).WithMany(t => t.NoteTags)
                .HasForeignKey(x => x.TagId).OnDelete(DeleteBehavior.Cascade);

            e.HasQueryFilter(x => x.Note!.UserId == CurrentUserId);
        });

        modelBuilder.Entity<TodoTag>(e =>
        {
            e.ToTable("TodoTag");
            e.HasKey(x => new { x.TodoId, x.TagId });
            e.HasIndex(x => x.TagId);

            e.HasOne(x => x.Todo).WithMany(t => t.Tags)
                .HasForeignKey(x => x.TodoId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Tag).WithMany(t => t.TodoTags)
                .HasForeignKey(x => x.TagId).OnDelete(DeleteBehavior.Cascade);

            e.HasQueryFilter(x => x.Todo!.UserId == CurrentUserId);
        });

        modelBuilder.Entity<JournalTag>(e =>
        {
            e.ToTable("JournalTag");
            e.HasKey(x => new { x.JournalEntryId, x.TagId });
            e.HasIndex(x => x.TagId);

            e.HasOne(x => x.JournalEntry).WithMany(j => j.Tags)
                .HasForeignKey(x => x.JournalEntryId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Tag).WithMany(t => t.JournalTags)
                .HasForeignKey(x => x.TagId).OnDelete(DeleteBehavior.Cascade);

            e.HasQueryFilter(x => x.JournalEntry!.UserId == CurrentUserId);
        });

        foreach (var property in modelBuilder.Model.GetEntityTypes()
            .SelectMany(type => type.GetProperties())
            .Where(property => property.ClrType == typeof(DateOnly) || property.ClrType == typeof(DateOnly?)))
        {
            property.SetValueConverter(property.ClrType == typeof(DateOnly)
                ? DateOnlyConverter
                : (ValueConverter)NullableDateOnlyConverter);
            property.SetColumnType("date");
        }


        modelBuilder.Entity<CategoryModule>(e =>
        {
            e.ToTable("CategoryModule");
            e.HasKey(x => x.Id);
            e.HasIndex(x => new { x.CategoryId, x.ModuleKey }).IsUnique();
            e.HasIndex(x => x.UserId);
            e.Property(x => x.ModuleKey).HasConversion<byte>();
            ConfigureBase(e);
            e.HasOne(x => x.Category).WithMany(c => c.Modules)
                .HasForeignKey(x => x.CategoryId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<GlobalModule>(e =>
        {
            e.ToTable("GlobalModule");
            e.HasKey(x => x.Id);
            e.HasIndex(x => new { x.UserId, x.ModuleKey }).IsUnique();
            e.Property(x => x.ModuleKey).HasConversion<byte>();
            e.Property(x => x.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
            e.Property(x => x.UpdatedAt).ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
        });

                ApplyUserScopeFilters(modelBuilder);
    }

    private void ApplyUserScopeFilters(ModelBuilder modelBuilder)
    {
        var userId = Expression.Property(Expression.Constant(this), nameof(CurrentUserId));

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (!typeof(IUserOwned).IsAssignableFrom(entityType.ClrType))
                continue;

            var parameter = Expression.Parameter(entityType.ClrType, "entity");
            var filter = Expression.Lambda(
                Expression.Equal(Expression.Property(parameter, nameof(IUserOwned.UserId)), userId),
                parameter);

            modelBuilder.Entity(entityType.ClrType).HasQueryFilter(filter);
        }
    }

    private static void ConfigureBase<TEntity>(EntityTypeBuilder<TEntity> e)
        where TEntity : BaseEntity
    {
        e.Property(x => x.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
        e.Property(x => x.UpdatedAt).ValueGeneratedOnAddOrUpdate()
            .HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
        e.HasIndex(x => x.IsDeleted);
        e.HasIndex(x => x.UserId);
    }

    private static void ConfigureCategoryRef<TEntity>(
        EntityTypeBuilder<TEntity> e,
        Expression<Func<Category, IEnumerable<TEntity>?>> principalNav)
        where TEntity : BaseEntity, ICategoryEntity
    {
        e.HasOne(x => x.Category).WithMany(principalNav)
            .HasForeignKey(x => x.CategoryId).OnDelete(DeleteBehavior.Cascade);
    }

    // Oracle's MySql.EntityFrameworkCore cannot materialize DateOnly; convert to DateTime at the provider
    // boundary and keep the column type at date so the schema stays a pure calendar date.
    private static readonly ValueConverter<DateOnly, DateTime> DateOnlyConverter = new(
        date => date.ToDateTime(TimeOnly.MinValue),
        value => DateOnly.FromDateTime(value));

    private static readonly ValueConverter<DateOnly?, DateTime?> NullableDateOnlyConverter = new(
        date => date.HasValue ? date.Value.ToDateTime(TimeOnly.MinValue) : null,
        value => value.HasValue ? DateOnly.FromDateTime(value.Value) : null);
}
