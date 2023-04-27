using System;
using System.Collections;
using System.Linq;
using MccSoft.Logging;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Diagnostics;
using Metalama.Framework.Eligibility;
using Microsoft.Extensions.Logging;

#nullable enable

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public class LogAttribute : OverrideMethodAspect, IAspect<INamedType>
{
    private readonly bool _withResult;

    public LogAttribute(bool withResult = false)
    {
        _withResult = withResult;
    }

    public override void BuildEligibility(IEligibilityBuilder<IMethod> builder)
    {
        base.BuildEligibility(builder);
        builder.MustNotBeStatic();
    }

    private static readonly DiagnosticDefinition<INamedType> _error1 =
        new("MY001", Severity.Error, "The type '{0}' must have a field named '_logger'.");

    private static readonly DiagnosticDefinition<IField> _error2 =
        new("MY002", Severity.Error, "The type of the field '{0}' must be 'TextWriter'.");

    public void BuildAspect(IAspectBuilder<INamedType> builder)
    {
        foreach (var method in builder.Target.Methods.Where(m => !m.IsStatic))
        {
            builder.Advice.Override(method, nameof(this.OverrideMethod));
        }
    }

    public override void BuildAspect(IAspectBuilder<IMethod> builder)
    {
        base.BuildAspect(builder);

        // Validate that the target file has a field named 'logger' of type TextWriter.
        var declaringType = builder.Target.DeclaringType;
        var loggerField = declaringType.Fields.OfName("_logger").SingleOrDefault();

        if (loggerField == null)
        {
            builder.Diagnostics.Report(_error1.WithArguments(declaringType), declaringType);
            builder.SkipAspect();
        }
        else if (!loggerField.Type.Is(typeof(ILogger)))
        {
            builder.Diagnostics.Report(_error2.WithArguments(loggerField), loggerField);
            builder.SkipAspect();
        }
    }

    [Template]
    public override dynamic? OverrideMethod()
    {
        var logger = (ILogger)meta.This._logger;
        object resultRef = null;

        var operationContext = new OperationContext();
        foreach (var p in meta.Target.Parameters.Where(p => p.RefKind != RefKind.Out))
        {
            // It's important not to call `.ToString()` on IQueryables.
            // It seems to call it on IEnumerable also have no sense.
            if (!(p.Value is IEnumerable))
            {
                operationContext.Add(
                    Field.Named(LogAttributePostProcess.PostProcessParameterName(p.Name)),
                    p.Value
                );
            }
        }

        using var log = logger.LogOperation(
            operationContext,
            _withResult ? () => resultRef : null,
            meta.Target.Method.Name
        );

        // Invoke the method.
        var result = meta.Proceed();

        if (!meta.Target.Method.ReturnType.Is(typeof(void)))
        {
            resultRef = (object)result;
        }

        return result;
    }
}
