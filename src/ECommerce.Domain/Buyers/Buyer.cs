using ECommerce.Domain.Exceptions;

namespace ECommerce.Domain.Buyers;

public sealed class Buyer
{
    private Buyer()
    {
    }

    public Buyer(Guid id, string name)
    {
        if (id == Guid.Empty)
        {
            throw new DomainException("Buyer Id cannot be empty.");
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException("Buyer Name is required.");
        }

        Id = id;
        Name = name.Trim();
        CreatedAtUtc = DateTime.UtcNow;
        UpdatedAtUtc = CreatedAtUtc;
    }

    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime UpdatedAtUtc { get; private set; }
    public DateTime? DeletedAtUtc { get; private set; }
}
