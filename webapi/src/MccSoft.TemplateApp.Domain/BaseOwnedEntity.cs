namespace MccSoft.TemplateApp.Domain;

public class BaseOwnedEntity : BaseEntity, IOwnedEntity
{
    public string OwnerId { get; private set; }
    public User Owner { get; }

    public void SetOwnerIdUnsafe(string ownerId)
    {
        OwnerId = ownerId;
    }
}
