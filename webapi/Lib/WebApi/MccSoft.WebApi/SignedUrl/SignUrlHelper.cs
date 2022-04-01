using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using MccSoft.LowLevelPrimitives;
using MccSoft.LowLevelPrimitives.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace MccSoft.WebApi.SignedUrl
{
    /// <summary>
    /// Helps to create and validate signatures used in Signed Urls
    /// </summary>
    public class SignUrlHelper
    {
        private readonly IOptions<SignUrlOptions> _signUrlOptions;
        private readonly IUserAccessor _userAccessor;
        private readonly Lazy<SymmetricSecurityKey> _securityKey;

        public const string UserIdClaimName = "id";
        public const string UrlParameterName = "sign";
        public const string HeaderName = "X-Sign";

        public SignUrlHelper(IOptions<SignUrlOptions> signUrlOptions, IUserAccessor userAccessor)
        {
            _signUrlOptions = signUrlOptions;
            _userAccessor = userAccessor;
            _securityKey = new Lazy<SymmetricSecurityKey>(
                () => new SymmetricSecurityKey(Encoding.ASCII.GetBytes(signUrlOptions.Value.Secret))
            );
        }

        public virtual string GenerateSignature(Claim[] additionalClaims = null)
        {
            IEnumerable<Claim> claims = CreateClaims();
            if (additionalClaims != null)
            {
                claims = claims.Union(additionalClaims);
            }

            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = new JwtSecurityToken(
                expires: DateTime.UtcNow.Add(_signUrlOptions.Value.TokenDuration),
                claims: claims,
                signingCredentials: new SigningCredentials(
                    _securityKey.Value,
                    SecurityAlgorithms.HmacSha256
                )
            );
            return tokenHandler.WriteToken(jwtToken);
        }

        protected virtual Claim[] CreateClaims()
        {
            return new Claim[] { new(UserIdClaimName, _userAccessor.GetUserId()), };
        }

        public virtual bool IsSignatureValid(HttpContext context)
        {
            return IsSignatureValid(context, out ClaimsPrincipal _);
        }

        public virtual bool IsSignatureValid(
            HttpContext context,
            out ClaimsPrincipal claimsPrincipal
        )
        {
            if (context.User.Identity?.IsAuthenticated == true)
            {
                claimsPrincipal = context.User;
                return true;
            }

            var signature = GetSignature(context);

            if (string.IsNullOrEmpty(signature))
            {
                throw new AccessDeniedException(
                    $"Parameter '{UrlParameterName}' is missing in request: '{context.Request.Path}{context.Request.QueryString}'"
                );
            }

            return IsSignatureValid(signature, out claimsPrincipal);
        }

        public virtual bool IsSignatureValid(string signature)
        {
            return IsSignatureValid(signature, out ClaimsPrincipal _);
        }

        public virtual bool IsSignatureValid(string signature, out ClaimsPrincipal claimsPrincipal)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            try
            {
                claimsPrincipal = tokenHandler.ValidateToken(
                    signature,
                    new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        ValidateIssuer = false,
                        ValidateAudience = false,
                        IssuerSigningKey = _securityKey.Value
                    },
                    out _
                );

                return ValidateClaims(claimsPrincipal);
            }
            catch (Exception e)
            {
                claimsPrincipal = null;
                return false;
            }
        }

        /// <summary>
        /// Validates claims.
        /// You could override it to provide custom validation rules.
        /// </summary>
        protected virtual bool ValidateClaims(ClaimsPrincipal claimsPrincipal)
        {
            return claimsPrincipal.FindFirstValue(UserIdClaimName) != null;
        }

        /// <summary>
        /// Returns the signature from the current request.
        /// Checks URL and Cookies for <see cref="UrlParameterName"/> by default.
        /// </summary>
        public virtual string GetSignature(HttpContext httpContext)
        {
            string signature = httpContext.Request.Query[UrlParameterName].ToString();

            if (string.IsNullOrEmpty(signature))
            {
                signature = httpContext.Request.Cookies[UrlParameterName];
            }

            if (string.IsNullOrEmpty(signature))
            {
                signature = httpContext.Request.Headers[HeaderName];
            }

            return signature;
        }
    }
}
