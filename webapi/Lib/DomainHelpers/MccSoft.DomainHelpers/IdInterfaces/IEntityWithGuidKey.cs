using System;

namespace MccSoft.DomainHelpers.IdInterfaces;

public interface IEntityWithGuidKey
{
    public Guid Id { get; }
}
