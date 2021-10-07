using System;
using System.Security.Claims;
using System.Threading.Tasks;
using MccSoft.LowLevelPrimitives.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;

namespace MccSoft.WebApi.SignedUrl
{
    public class ValidateSignedUrlAttribute : ActionFilterAttribute, IAllowAnonymous
    {
        public string ClaimType { get; }
        public Predicate<string> ClaimValuePredicate { get; }
        public string ClaimValue { get; }

        /// <summary>
        /// Attribute that shall be added to Actions that are to be secured by a SignedUrl
        /// Signature is expected to be in `sign` URL parameter (e.g. http://localhost/images/product/1?sign=blablabla
        /// or Cookie with name `sign`.
        /// </summary>
        /// <param name="claimType">Claim that is required to be present in sign url for authorization to succeed</param>
        /// <param name="claimValue">Value of the claim. If null just the presence of Claim will be checked</param>
        public ValidateSignedUrlAttribute(
            string claimType = SignUrlHelper.UserIdClaimName,
            string claimValue = null
        ) {
            ClaimType = claimType;
            ClaimValue = claimValue;
        }

        /// <summary>
        /// Attribute that shall be added to Actions that are to be secured by a SignedUrl
        /// Signature is expected to be in `sign` URL parameter (e.g. http://localhost/images/product/1?sign=blablabla
        /// </summary>
        /// <param name="claimType">Claim that is required to be present in sign url for authorization to succeed</param>
        /// <param name="claimValuePredicate">Predicate to check the Value of the claim.</param>
        public ValidateSignedUrlAttribute(
            string claimType,
            Predicate<string> claimValuePredicate = null
        ) {
            ClaimType = claimType;
            ClaimValuePredicate = claimValuePredicate;
        }

        public override async Task OnActionExecutionAsync(
            ActionExecutingContext context,
            ActionExecutionDelegate next
        ) {
            var httpContext = context.HttpContext;
            SignUrlHelper helper = httpContext.RequestServices.GetRequiredService<SignUrlHelper>();

            bool isValid = helper.IsSignatureValid(
                httpContext,
                out ClaimsPrincipal claimsPrincipal
            );

            if (!isValid)
            {
                throw new AccessDeniedException($"Signature is invalid");
            }

            if (!string.IsNullOrEmpty(ClaimType))
            {
                Claim? claimValue = claimsPrincipal.FindFirst(ClaimType);

                if (claimValue == null)
                {
                    throw new AccessDeniedException(
                        $"Claim '{ClaimType}' is required, but wasn't found in token."
                    );
                }

                if (!string.IsNullOrEmpty(ClaimValue))
                {
                    if (claimValue.Value != ClaimValue)
                    {
                        throw new AccessDeniedException(
                            $"Claim '{ClaimType}' has value '{claimValue.Value}', while it was required to have '{ClaimValue}'."
                        );
                    }
                }
                else if (ClaimValuePredicate != null)
                {
                    if (ClaimValuePredicate.Invoke(claimValue.Value) != true)
                    {
                        throw new AccessDeniedException(
                            $"Claim '{ClaimType}' has value '{claimValue.Value}', which doesn't suit the predicate."
                        );
                    }
                }
            }

            await base.OnActionExecutionAsync(context, next);
        }
    }
}
