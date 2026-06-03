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
            throw new DomainException("Product Id não pode ser vazio.");
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException("Product Name é obrigatório.");
        }

        if (price <= 0)
        {
            throw new DomainException("Product Price deve ser maior que zero.");
        }

        Id = id;
        Name = name.Trim();
        Price = price;
    }

    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public decimal Price { get; private set; }
}
