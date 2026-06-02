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
            throw new DomainException("Buyer Id não pode ser vazio.");
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException("Buyer Name é obrigatório.");
        }

        Id = id;
        Name = name.Trim();
    }

    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
}
