using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Localization;

namespace MccSoft.TemplateApp.App.Utils.Localization;

public class LocalizableIdentityErrorDescriber : IdentityErrorDescriber
{
    private readonly IStringLocalizer _localizer;

    public LocalizableIdentityErrorDescriber(
        IStringLocalizer<LocalizableIdentityErrorDescriber> localizer
    )
    {
        _localizer = localizer;
    }

    public override IdentityError DuplicateEmail(string email)
    {
        return new IdentityError()
        {
            Code = nameof(DuplicateEmail),
            Description = _localizer["IdentityErrors:DuplicateEmail", new { email }]
        };
    }

    public override IdentityError DefaultError()
    {
        return new IdentityError
        {
            Code = nameof(DefaultError),
            Description = _localizer["IdentityErrors:DefaultError", new { }]
        };
    }

    public override IdentityError ConcurrencyFailure()
    {
        return new IdentityError
        {
            Code = nameof(ConcurrencyFailure),
            Description = _localizer["IdentityErrors:DefaultError", new { }]
        };
    }

    public override IdentityError PasswordMismatch()
    {
        return new IdentityError
        {
            Code = nameof(PasswordMismatch),
            Description = _localizer["IdentityErrors:PasswordMismatch", new { }]
        };
    }

    public override IdentityError InvalidToken()
    {
        return new IdentityError
        {
            Code = nameof(InvalidToken),
            Description = _localizer["IdentityErrors:InvalidToken", new { }]
        };
    }

    public override IdentityError LoginAlreadyAssociated()
    {
        return new IdentityError
        {
            Code = nameof(LoginAlreadyAssociated),
            Description = _localizer["IdentityErrors:LoginAlreadyAssociated", new { }]
        };
    }

    public override IdentityError InvalidUserName(string userName)
    {
        return new IdentityError
        {
            Code = nameof(InvalidUserName),
            Description = _localizer["IdentityErrors:InvalidUserName", new { userName }]
        };
    }

    public override IdentityError InvalidEmail(string email)
    {
        return new IdentityError
        {
            Code = nameof(InvalidEmail),
            Description = _localizer["IdentityErrors:InvalidEmail", new { email }]
        };
    }

    public override IdentityError DuplicateUserName(string userName)
    {
        return new IdentityError
        {
            Code = nameof(DuplicateUserName),
            Description = _localizer["IdentityErrors:DuplicateUserName", new { userName }]
        };
    }

    public override IdentityError InvalidRoleName(string role)
    {
        return new IdentityError
        {
            Code = nameof(InvalidRoleName),
            Description = _localizer["IdentityErrors:InvalidRoleName", new { role }]
        };
    }

    public override IdentityError DuplicateRoleName(string role)
    {
        return new IdentityError
        {
            Code = nameof(DuplicateRoleName),
            Description = _localizer["IdentityErrors:DuplicateRoleName", new { role }]
        };
    }

    public override IdentityError UserAlreadyHasPassword()
    {
        return new IdentityError
        {
            Code = nameof(UserAlreadyHasPassword),
            Description = _localizer["IdentityErrors:UserAlreadyHasPassword", new { }]
        };
    }

    public override IdentityError UserLockoutNotEnabled()
    {
        return new IdentityError
        {
            Code = nameof(UserLockoutNotEnabled),
            Description = _localizer["IdentityErrors:UserLockoutNotEnabled", new { }]
        };
    }

    public override IdentityError UserAlreadyInRole(string role)
    {
        return new IdentityError
        {
            Code = nameof(UserAlreadyInRole),
            Description = _localizer["IdentityErrors:UserAlreadyInRole", new { role }]
        };
    }

    public override IdentityError UserNotInRole(string role)
    {
        return new IdentityError
        {
            Code = nameof(UserNotInRole),
            Description = _localizer["IdentityErrors:UserNotInRole", new { role }]
        };
    }

    public override IdentityError PasswordTooShort(int length)
    {
        return new IdentityError
        {
            Code = nameof(PasswordTooShort),
            Description = _localizer["IdentityErrors:PasswordTooShort", new { length }]
        };
    }

    public override IdentityError PasswordRequiresNonAlphanumeric()
    {
        return new IdentityError
        {
            Code = nameof(PasswordRequiresNonAlphanumeric),
            Description = _localizer["IdentityErrors:PasswordRequiresNonAlphanumeric", new { }]
        };
    }

    public override IdentityError PasswordRequiresDigit()
    {
        return new IdentityError
        {
            Code = nameof(PasswordRequiresDigit),
            Description = _localizer["IdentityErrors:PasswordRequiresDigit", new { }]
        };
    }

    public override IdentityError PasswordRequiresLower()
    {
        return new IdentityError
        {
            Code = nameof(PasswordRequiresLower),
            Description = _localizer["IdentityErrors:PasswordRequiresLower", new { }]
        };
    }

    public override IdentityError PasswordRequiresUpper()
    {
        return new IdentityError
        {
            Code = nameof(PasswordRequiresUpper),
            Description = _localizer["IdentityErrors:PasswordRequiresUpper", new { }]
        };
    }

    public override IdentityError PasswordRequiresUniqueChars(int number)
    {
        return new IdentityError
        {
            Code = nameof(PasswordRequiresUpper),
            Description = _localizer["IdentityErrors:PasswordRequiresUniqueChars", new { number }]
        };
    }

    public override IdentityError RecoveryCodeRedemptionFailed()
    {
        return new IdentityError
        {
            Code = nameof(PasswordRequiresUpper),
            Description = _localizer["IdentityErrors:RecoveryCodeRedemptionFailed", new { }]
        };
    }
}
