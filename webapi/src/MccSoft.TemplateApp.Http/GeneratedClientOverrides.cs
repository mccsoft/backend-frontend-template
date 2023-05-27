using System;
using System.Linq;

namespace MccSoft.TemplateApp.Http.Generated;

public partial class ApiException<TResult>
{
    private bool _entered;

    public override string ToString()
    {
        if (Result is ValidationProblemDetails validationProblemDetails)
        {
            return string.Join(
                    "; ",
                    validationProblemDetails.Errors?.Select(
                        x => x.Key + ": " + string.Join(", ", x.Value)
                    ) ?? Array.Empty<string>()
                ) + validationProblemDetails.Detail;
        }
        else if (Result is ProblemDetails problemDetails)
        {
            return problemDetails.Detail;
        }

        try
        {
            _entered = true;
            return base.ToString();
        }
        finally
        {
            _entered = false;
        }
    }

    public override string Message => _entered ? base.Message : ToString();
}
