using System.Collections.Generic;
using Serilog.Core;
using Serilog.Events;

namespace MccSoft.Logging
{
    /// <summary>
    /// Enricher to help Serilog serialize operation context dictionary correctly.
    /// </summary>
    public class OperationContextEnricher : ILogEventEnricher
    {
        private readonly OperationContext _operationContext;

        public OperationContextEnricher(OperationContext operationContext, string operationName)
        {
            _operationContext = operationContext;
            _operationContext.Add(Field.Method, operationName);
        }

        /// <inheritdoc />
        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            foreach (KeyValuePair<Field, object> p in _operationContext)
            {
                logEvent.AddPropertyIfAbsent(
                    new LogEventProperty(p.Key.PrefixedName, new ScalarValue(p.Value))
                );
            }
        }
    }
}
