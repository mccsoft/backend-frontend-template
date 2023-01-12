using System;
using System.Linq;
using System.Linq.Expressions;

namespace MccSoft.DomainHelpers;

/// <summary>
/// Represents a filter over entities.
/// </summary>
/// <typeparam name="TEntity">
/// The entity type.
/// </typeparam>
public class Specification<TEntity>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Specification{TEntity}" /> class.
    /// </summary>
    /// <param name="name">The name of this specification (for better diagnostic).</param>
    /// <param name="predicate">The predicate.</param>
    /// <param name="parameters">The parameters.</param>
    public Specification(
        string name,
        Expression<Func<TEntity, bool>> predicate,
        params object[] parameters
    )
    {
        Name = name;
        Parameters = parameters;
        Predicate = predicate;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Specification{TEntity}" /> class.
    /// </summary>
    protected Specification()
    {
        Name = GetType().Name;
        Parameters = new object[0];
        Predicate = e => false;
    }

    /// <summary>
    /// Gets the name of this specification.
    /// </summary>
    public string Name { get; protected set; }

    /// <summary>
    /// Gets the predicate which returns true
    /// for entities satisfying this specification.
    /// </summary>
    public Expression<Func<TEntity, bool>> Predicate { get; }

    /// <summary>
    /// Gets the parameters of this specification.
    /// </summary>
    protected object[] Parameters { get; private set; }

    /// <summary>
    /// Returns a string that represents the current object.
    /// </summary>
    /// <returns>
    /// A string that represents the current object.
    /// </returns>
    public override string ToString()
    {
        string parameters = string.Join(", ", Parameters.Select(x => x ?? "(null)"));
        return $"{Name}({parameters})";
    }
}
