namespace MccSoft.TemplateApp.Domain;

public class BaseOwnedEntity : BaseEntity, IOwnedEntity
{
    public string OwnerId { get; private set; } = null!;
    public User Owner { get; } = null!;

    public void SetOwnerIdUnsafe(string ownerId)
    {
        OwnerId = ownerId;
    }
}
