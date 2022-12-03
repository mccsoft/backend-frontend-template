using System.Collections.Generic;
using MccSoft.DomainHelpers;
using MccSoft.DomainHelpers.DomainEvents;
using MccSoft.DomainHelpers.DomainEvents.Events;
using Microsoft.AspNetCore.Identity;

namespace MccSoft.TemplateApp.Domain;

public class User : IdentityUser, ITenantEntity
{
    public int TenantId { get; private set; }

    public Tenant Tenant { get; private set; }

    public string FirstName { get; private set; } = "";

    public string LastName { get; private set; } = "";

    /// <summary>
    /// Needed for Entity Framework, keep empty.
    /// </summary>
    public User() { }

    /// <summary>
    /// Constructor to initialize User entity.
    /// </summary>
    public User(string email)
    {
        UserName = email;
        Email = email;
    }

    /// <summary>
    /// Constructor to initialize User entity with first and last name.
    /// </summary>
    /// <param name="firstName">First name of the user.</param>
    /// <param name="lastName">Last name of the user.</param>
    /// <param name="email">Identifier of the user.</param>
    public User(string firstName, string lastName, string email)
    {
        UserName = email;
        Email = email;
        FirstName = firstName;
        LastName = lastName;
    }

    /// <summary>
    /// Where it makes sense, we could also use DDD approach (so that Entity properties have only private setters,
    /// and modification happens via invoking a method on an object).
    /// </summary>
    public void ChangeFirstNameAndLastName(string firstName, string lastName)
    {
        FirstName = firstName;
        LastName = lastName;

        // In DDD methods, you could use Domain Events if you need to call some Service from main App project.
        // E.g. if you want to send an Email, you could create 'SendEmailDomainEvent' class
        // and add 'SendEmailDomainEventHandler' in the main App (where you have access to IMailSender and everything).
        // (e.g. you could send an email to the user when his first/last name changes).

        // In this example we are just logging the fact that firstname/lastname changes.
        // The handler of this domain event (`LogDomainEventHandler`) resides in main App.
        // You could use this as an example to implement your own events.
        // This particular example is actually BAD, you should NOT log something from every DDD method unless you really want to.
        AddEvent(
            LogDomainEvent.Info(
                "User '{Id}' FirstName/LastName changed to {firstName}, {lastName}",
                Id,
                firstName,
                lastName
            )
        );
    }

    /// <summary>
    /// Creates a specification that is satisfied by a user having the specified id.
    /// </summary>
    /// <param name="id">The user id.</param>
    /// <returns>The created specification.</returns>
    public static Specification<User> HasId(string id) => new(nameof(HasId), p => p.Id == id, id);

    public static Specification<User> HasEmail(string email) =>
        new(nameof(HasEmail), p => p.NormalizedEmail == email.ToUpper(), email);

    #region Domain Events

    private List<IDomainEvent>? _domainEvents;
    public IReadOnlyList<IDomainEvent>? DomainEvents => _domainEvents;

    public void AddEvent(IDomainEvent domainEvent, bool removeEventsOfSameType = false)
    {
        _domainEvents ??= new List<IDomainEvent>();
        if (removeEventsOfSameType)
        {
            _domainEvents.RemoveAll(x => x.GetType() == domainEvent.GetType());
        }

        _domainEvents.Add(domainEvent);
    }

    public void ClearDomainEvents()
    {
        _domainEvents?.Clear();
    }

    public void SetTenantIdUnsafe(int tenantId)
    {
        TenantId = tenantId;
    }
    #endregion
}
