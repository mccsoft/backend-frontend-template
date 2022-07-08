using System;
using System.Linq;
using System.Threading.Tasks;
using MccSoft.TemplateApp.Http.Generated;
using Newtonsoft.Json;

namespace MccSoft.TemplateApp.Http.Generated
{
    public partial class ApiException<TResult>
    {
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

            return base.ToString();
        }

        public override string Message => ToString();
    }
}
