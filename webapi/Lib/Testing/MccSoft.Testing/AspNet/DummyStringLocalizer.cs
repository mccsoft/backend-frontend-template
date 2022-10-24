using System.Collections.Generic;
using Microsoft.Extensions.Localization;

namespace MccSoft.Testing.AspNet;

public class DummyStringLocalizer : DummyStringLocalizer<object> { }

public class DummyStringLocalizer<T> : IStringLocalizer<T>
{
    public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures)
    {
        return new List<LocalizedString>();
    }

    public LocalizedString this[string name] => new LocalizedString(name, name);

    public LocalizedString this[string name, params object[] arguments] =>
        new LocalizedString(name, string.Format(name, arguments));
}
