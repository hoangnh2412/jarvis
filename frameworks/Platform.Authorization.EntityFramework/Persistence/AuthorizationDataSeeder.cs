namespace Platform.Authorization.EntityFramework.Persistence;

using Platform.Authorization.Services;

/// <summary>
/// Seed mặc định platform + mọi đóng góp <c>AddPlatformAuthorizationSeed(...)</c>, kiểm tra operand
/// của từng rule với <see cref="Operands.IOperandRegistry"/>. Host gọi sau khi schema Identity đã được migrate.
/// </summary>
public sealed class AuthorizationDataSeeder
{
    private readonly IdentityDbContextBase _context;
    private readonly OperandValidator _validator;
    private readonly IEnumerable<AuthorizationSeedContribution> _contributions;

    public AuthorizationDataSeeder(
        IdentityDbContextBase context,
        OperandValidator validator,
        IEnumerable<AuthorizationSeedContribution> contributions)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _validator = validator ?? throw new ArgumentNullException(nameof(validator));
        _contributions = contributions ?? throw new ArgumentNullException(nameof(contributions));
    }

    public AuthorizationSeedCatalog BuildCatalog() =>
        AuthorizationSeeder.BuildCatalog(_contributions.Select(c => c.Configure));

    public Task SeedAsync(CancellationToken cancellationToken = default) =>
        _context.SeedAsync(BuildCatalog(), _validator, cancellationToken);
}
