using Microsoft.EntityFrameworkCore;
using ActualLab.Fusion.Authentication.Services;
using ActualLab.Fusion.EntityFramework;
using ActualLab.Fusion.EntityFramework.Operations;
using ActualLab.Fusion.Extensions.Services;

namespace Templates.TodoApp.Services;

public class AppDbContext(DbContextOptions options) : DbContextBase(options)
{
    // ActualLab.Fusion.EntityFramework tables
    public DbSet<DbUser<string>> Users { get; protected set; } = null!;
    public DbSet<DbUserIdentity<string>> UserIdentities { get; protected set; } = null!;
    public DbSet<DbSessionInfo<string>> Sessions { get; protected set; } = null!;
    public DbSet<DbKeyValue> KeyValues { get; protected set; } = null!;
    public DbSet<DbOperation> Operations { get; protected set; } = null!;
}
