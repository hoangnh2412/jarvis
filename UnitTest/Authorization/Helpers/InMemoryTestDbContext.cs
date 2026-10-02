using Microsoft.EntityFrameworkCore;
using Platform.Authorization.EntityFramework.Persistence;

namespace UnitTest.Authorization.Helpers;

/// <summary>
/// In-memory test DbContext for Platform.Authorization tests.
/// Extends IdentityDbContextBase to inherit Authorization entities (Role, Policy, PolicyRule).
/// </summary>
public class InMemoryTestDbContext : IdentityDbContextBase
{
    public InMemoryTestDbContext(DbContextOptions options) : base(options)
    {
    }
}
