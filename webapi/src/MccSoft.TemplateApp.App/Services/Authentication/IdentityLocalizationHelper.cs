using System.Collections.Generic;
using System.Linq;
using MccSoft.LowLevelPrimitives.Exceptions;
using Microsoft.AspNetCore.Identity;

namespace MccSoft.TemplateApp.App.Services.Authentication;

public static class IdentityLocalizationHelper
{
    /*at the moment the following errors could appear from UserService methods
     * (see https://github.com/aspnet/AspNetIdentity/blob/master/src/Microsoft.AspNet.Identity.Core/Resources.resx)
     */

    private static bool IsOldPasswordError(string code)
    {
        return code == "PasswordMismatch";
    }

    private static HashSet<string> _newPasswordErrors =
        new()
        {
            "PasswordTooShort",
            "PasswordRequiresUniqueChars",
            "PasswordRequiresNonAlphanumeric",
            "PasswordRequiresDigit",
            "PasswordRequiresLower",
            "PasswordRequiresUpper",
        };

    private static bool IsNewPasswordError(string code)
    {
        return _newPasswordErrors.Contains(code);
    }

    private static HashSet<string> _usernameErrors =
        new()
        {
            "DuplicateEmail",
            "DuplicateUserName",
            "InvalidEmail",
            "InvalidUserName",
            "UserNameNotFound",
            "UserIdNotFound",
            "UserNameNotFound",
        };

    private static bool IsUsernameError(string code)
    {
        return _usernameErrors.Contains(code);
    }

    public static void ParseIdentityError(
        IdentityResult result,
        string? oldPasswordFieldName = null,
        string? newPasswordFieldName = null,
        string? usernameFieldName = null
    )
    {
        IdentityError firstError = result.Errors.First();
        if (!string.IsNullOrEmpty(oldPasswordFieldName) && IsOldPasswordError(firstError.Code))
            throw new ValidationException(oldPasswordFieldName, firstError.Description);
        if (!string.IsNullOrEmpty(newPasswordFieldName) && IsNewPasswordError(firstError.Code))
            throw new ValidationException(newPasswordFieldName, firstError.Description);
        if (!string.IsNullOrEmpty(usernameFieldName) && IsUsernameError(firstError.Code))
            throw new ValidationException(usernameFieldName, firstError.Description);
        throw new ValidationException(firstError.Description);
    }
}
