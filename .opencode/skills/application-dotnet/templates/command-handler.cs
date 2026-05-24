// Ví dụ — điều chỉnh theo contract Jarvis.Application của repo

namespace {Product}.Application.Orders;

public sealed record CreateOrderCommand(string Name) : ICommand<Guid>;

public sealed class CreateOrderHandler : ICommandHandler<CreateOrderCommand, Guid>
{
    public Task<Guid> HandleAsync(CreateOrderCommand command, CancellationToken cancellationToken)
    {
        // UoW / repository
        return Task.FromResult(Guid.NewGuid());
    }
}
