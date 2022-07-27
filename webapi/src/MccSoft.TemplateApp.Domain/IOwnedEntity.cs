namespace MccSoft.TemplateApp.Domain;

public interface IOwnedEntity
{
    public string OwnerId { get; }
    public User Owner { get; }
    public void SetOwnerIdUnsafe(string ownerId);
}
