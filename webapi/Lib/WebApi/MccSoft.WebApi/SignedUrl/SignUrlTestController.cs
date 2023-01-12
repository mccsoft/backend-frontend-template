using System;
using System.Net.Http.Headers;
using System.Security.Claims;
using MccSoft.LowLevelPrimitives.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;

namespace MccSoft.WebApi.SignedUrl;

// [ApiExplorerSettings(IgnoreApi = true)]
[Authorize]
public class SignUrlTestController
{
    private readonly IHostEnvironment _hostEnvironment;
    private readonly SignUrlHelper _signUrlHelper;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public SignUrlTestController(
        IHostEnvironment hostEnvironment,
        SignUrlHelper signUrlHelper,
        IHttpContextAccessor httpContextAccessor
    )
    {
        _hostEnvironment = hostEnvironment;
        _signUrlHelper = signUrlHelper;
        _httpContextAccessor = httpContextAccessor;
    }
    /// <summary>
    /// It's preferable to use `imageGuid` in URL (not the productId), because it will be cached by the browser.
    /// And when image for Product is updated, the URL of the image should be changed (to avoid displaying old image from cache).
    /// </summary>
    // [HttpGet("product/image/{imageGuid}")]
    // [ValidateSignedUrl]
    // public FileResult GetProductImage(string imageGuid)
    // {
    //     return new FileContentResult(new byte[] { 0, 1, 2 }, "image/jpg");
    // }
    //
    // /// <summary>
    // /// It's preferable to use `imageGuid` in URL (not the productId), because it will be cached by the browser.
    // /// And when image for Product is updated, the URL of the image should be changed (to avoid displaying old image from cache).
    // /// </summary>
    // [HttpGet("product/image/details/{imageGuid}")]
    // [ValidateSignedUrl]
    // public FileResult GetProductImageWithAdvancedUserValidation(string imageGuid)
    // {
    //     if (!_signUrlHelper.IsSignatureValid(_httpContextAccessor.HttpContext,
    //         out var claimsPrincipal))
    //     {
    //         throw new AccessDeniedException("Access denied");
    //     }
    //
    //     var userId = claimsPrincipal.FindFirstValue(SignUrlHelper.UserIdClaimName);
    //
    //     // you could add any custom logic to check if user is allowed to access the file.
    //     if (userId != "admin")
    //     {
    //         throw new AccessDeniedException("Access denied");
    //     }
    //
    //     return new FileContentResult(new byte[] { 0, 1, 2 }, "image/jpg");
    // }

    // [HttpGet("signature")]
    // public string GetSignature([FromServices] SignUrlHelper signUrlHelper)
    // {
    //     if (!_hostEnvironment.IsDevelopment())
    //     {
    //         throw new NotSupportedException(
    //             "GET signature is not supported on Production by default. Please, implement your own method of getting the signature and secure it properly.");
    //     }
    //
    //     return signUrlHelper.GenerateSignature();
    // }
    //
    // [HttpPost("signature/cookie")]
    // public ActionResult SetSignatureCookie([FromServices] SignUrlHelper signUrlHelper,
    //     [FromServices] IHttpContextAccessor httpContextAccessor)
    // {
    //     if (!_hostEnvironment.IsDevelopment())
    //     {
    //         throw new NotSupportedException(
    //             "Setting signature cookie is not supported on Production by default. Please, implement your own method of getting the signature and secure it properly.");
    //     }
    //
    //     string signature = signUrlHelper.GenerateSignature();
    //
    //     httpContextAccessor.HttpContext!.Response.Cookies.Append(
    //         SignUrlHelper.UrlParameterName, signature);
    //     return new OkResult();
    // }
}
