using ECommerce.Domain.Exceptions;

namespace ECommerce.Domain.Products;

public sealed class Product
{
    private Product()
    {
    }

    public Product(Guid id, string name, decimal price)
    {
        if (id == Guid.Empty)
        {
            throw new DomainException("Product Id cannot be empty.");
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException("Product Name is required.");
        }

        if (price <= 0)
        {
            throw new DomainException("Product Price must be greater than zero.");
        }

        Id = id;
        Name = name.Trim();
        Price = price;
        CreatedAtUtc = DateTime.UtcNow;
        UpdatedAtUtc = CreatedAtUtc;
    }

    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public decimal Price { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime UpdatedAtUtc { get; private set; }
    public DateTime? DeletedAtUtc { get; private set; }
}
