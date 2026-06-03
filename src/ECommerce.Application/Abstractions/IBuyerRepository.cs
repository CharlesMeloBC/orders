using ECommerce.Domain.Buyers;

namespace ECommerce.Application.Abstractions;

public interface IBuyerRepository
{
    Task<Buyer?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    void Add(Buyer buyer);
}
