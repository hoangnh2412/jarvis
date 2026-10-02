namespace Platform.Modules.IdentityAdmin.Queries;

using Platform.Authentication;
using Platform.DDD.Application.Contracts.Queries;
using Platform.DDD.Domain.Services;
using Platform.Authorization.Abstractions;

public sealed class GetPoliciesQueryHandler : IAsyncQueryHandler<GetPoliciesQuery, IReadOnlyCollection<PolicySummaryDto>>
{
    private readonly IPolicyProvider _policyProvider;
    private readonly ICurrentUser<CurrentUserInfo> _currentUser;

    public GetPoliciesQueryHandler(
        IPolicyProvider policyProvider,
        ICurrentUser<CurrentUserInfo> currentUser)
    {
        _policyProvider = policyProvider ?? throw new ArgumentNullException(nameof(policyProvider));
        _currentUser = currentUser ?? throw new ArgumentNullException(nameof(currentUser));
    }

    public async Task<IReadOnlyCollection<PolicySummaryDto>> HandleAsync(GetPoliciesQuery query, CancellationToken cancellationToken = default)
    {
        var actor = await _currentUser.GetAsync(cancellationToken);
        if (actor?.UserId == null)
            throw new UnauthorizedAccessException("Current user not found");

        var policies = await _policyProvider.GetPoliciesByUserAsync(actor.UserId.Value, cancellationToken);
        return policies.Select(p => new PolicySummaryDto
        {
            Code = p.Code,
            Name = p.Name,
            ResourceType = p.ResourceType,
            Action = p.Action
        }).ToList();
    }
}
