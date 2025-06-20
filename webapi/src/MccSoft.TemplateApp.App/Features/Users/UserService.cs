using MccSoft.LowLevelPrimitives;
using MccSoft.LowLevelPrimitives.Exceptions;
using MccSoft.PersistenceHelpers;
using MccSoft.TemplateApp.App.Features.Users.Dto;
using MccSoft.TemplateApp.App.Services.Authentication;
using MccSoft.TemplateApp.Domain;
using MccSoft.TemplateApp.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace MccSoft.TemplateApp.App.Features.Users;

public class UserService
{
    private readonly ILogger<UserService> _logger;
    private readonly TemplateAppDbContext _dbContext;
    private readonly UserManager<User> _userManager;
    private readonly IUserAccessor _userAccessor;
    private readonly IStringLocalizer _stringLocalizer;

    public UserService(
        TemplateAppDbContext dbContext,
        UserManager<User> userManager,
        IUserAccessor userAccessor,
        IStringLocalizer stringLocalizer,
        ILogger<UserService> logger
    )
    {
        _dbContext = dbContext;
        _userManager = userManager;
        _userAccessor = userAccessor;
        _stringLocalizer = stringLocalizer;
        _logger = logger;
    }

    public async Task<CurrentUserDto> GetCurrentUserInfo()
    {
        var userId = _userAccessor.GetUserId();
        var user = await _dbContext.Users.FirstAsync(x => x.Id == userId);

        var permissions = await GetCurrentUserPermissions();

        return new CurrentUserDto()
        {
            Id = userId,
            Username = user.UserName ?? "",
            Nickname = $"{user.FirstName} {user.LastName}",
            Permissions = permissions.ToList(),
        };
    }

    public async Task<ISet<string>> GetCurrentUserPermissions()
    {
        return new HashSet<string>();
    }

    public async Task ResetPassword(ResetPasswordDto dto)
    {
        var user = await _userManager.FindByNameAsync(dto.Username);
        if (user is null)
            throw new PersistenceResourceNotFoundException(
                _stringLocalizer[
                    "CustomValidationErrors:user.user_not_found_by_login",
                    new { login = dto.Username }
                ]
            );

        // user sent the same password
        if (await _userManager.CheckPasswordAsync(user, dto.NewPassword))
            throw new ValidationException(
                nameof(dto.NewPassword),
                _stringLocalizer["CustomValidationErrors:user.password_reuse_forbidden"]
            );

        var result = await _userManager.ResetPasswordAsync(user, dto.Token, dto.NewPassword);

        if (!result.Succeeded)
        {
            IdentityLocalizationHelper.ParseIdentityError(
                result,
                newPasswordFieldName: nameof(ResetPasswordDto.NewPassword)
            );
        }

        await _dbContext.SaveChangesAsync();
    }

    public async Task ChangePassword(ChangePasswordDto model)
    {
        string userId = _userAccessor.GetUserId();
        User user = await _dbContext.Users.GetOne(User.HasId(userId));
        var result = await _userManager.ChangePasswordAsync(
            user,
            model.OldPassword,
            model.NewPassword
        );

        if (!result.Succeeded)
        {
            IdentityLocalizationHelper.ParseIdentityError(
                result,
                oldPasswordFieldName: nameof(model.OldPassword),
                newPasswordFieldName: nameof(model.NewPassword)
            );
        }

        await _dbContext.SaveChangesAsync();
    }

    public async Task ChangePasswordByAdmin(string userId, CreatePasswordDto dto)
    {
        User user = await _dbContext.Users.GetOne(User.HasId(userId));

        string passwordResetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
        IdentityResult result = await _userManager.ResetPasswordAsync(
            user,
            passwordResetToken,
            dto.Password
        );

        if (!result.Succeeded)
        {
            IdentityLocalizationHelper.ParseIdentityError(
                result,
                newPasswordFieldName: nameof(dto.Password)
            );
        }
    }
}
