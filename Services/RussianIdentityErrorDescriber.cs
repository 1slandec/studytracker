using Microsoft.AspNetCore.Identity;

namespace StudyTracker.Services;

public class RussianIdentityErrorDescriber : IdentityErrorDescriber
{
    public override IdentityError DefaultError()
    {
        return Error(nameof(DefaultError), "Произошла ошибка.");
    }

    public override IdentityError ConcurrencyFailure()
    {
        return Error(nameof(ConcurrencyFailure), "Данные были изменены другим процессом. Повторите попытку.");
    }

    public override IdentityError PasswordMismatch()
    {
        return Error(nameof(PasswordMismatch), "Неверный пароль.");
    }

    public override IdentityError InvalidToken()
    {
        return Error(nameof(InvalidToken), "Недействительный код подтверждения.");
    }

    public override IdentityError LoginAlreadyAssociated()
    {
        return Error(nameof(LoginAlreadyAssociated), "Этот внешний вход уже связан с другим аккаунтом.");
    }

    public override IdentityError InvalidUserName(string? userName)
    {
        return Error(nameof(InvalidUserName), $"Имя пользователя '{userName}' недопустимо.");
    }

    public override IdentityError InvalidEmail(string? email)
    {
        return Error(nameof(InvalidEmail), $"Email '{email}' недопустим.");
    }

    public override IdentityError DuplicateUserName(string userName)
    {
        return Error(nameof(DuplicateUserName), $"Пользователь '{userName}' уже существует.");
    }

    public override IdentityError DuplicateEmail(string email)
    {
        return Error(nameof(DuplicateEmail), $"Email '{email}' уже используется.");
    }

    public override IdentityError InvalidRoleName(string? role)
    {
        return Error(nameof(InvalidRoleName), $"Роль '{role}' недопустима.");
    }

    public override IdentityError DuplicateRoleName(string role)
    {
        return Error(nameof(DuplicateRoleName), $"Роль '{role}' уже существует.");
    }

    public override IdentityError UserAlreadyHasPassword()
    {
        return Error(nameof(UserAlreadyHasPassword), "У пользователя уже задан пароль.");
    }

    public override IdentityError UserLockoutNotEnabled()
    {
        return Error(nameof(UserLockoutNotEnabled), "Блокировка для этого пользователя не включена.");
    }

    public override IdentityError UserAlreadyInRole(string role)
    {
        return Error(nameof(UserAlreadyInRole), $"Пользователь уже находится в роли '{role}'.");
    }

    public override IdentityError UserNotInRole(string role)
    {
        return Error(nameof(UserNotInRole), $"Пользователь не находится в роли '{role}'.");
    }

    public override IdentityError PasswordTooShort(int length)
    {
        return Error(nameof(PasswordTooShort), $"Пароль должен быть не короче {length} символов.");
    }

    public override IdentityError PasswordRequiresUniqueChars(int uniqueChars)
    {
        return Error(nameof(PasswordRequiresUniqueChars), $"Пароль должен содержать минимум {uniqueChars} разных символов.");
    }

    public override IdentityError PasswordRequiresNonAlphanumeric()
    {
        return Error(nameof(PasswordRequiresNonAlphanumeric), "Пароль должен содержать хотя бы один специальный символ.");
    }

    public override IdentityError PasswordRequiresDigit()
    {
        return Error(nameof(PasswordRequiresDigit), "Пароль должен содержать хотя бы одну цифру.");
    }

    public override IdentityError PasswordRequiresLower()
    {
        return Error(nameof(PasswordRequiresLower), "Пароль должен содержать хотя бы одну строчную букву.");
    }

    public override IdentityError PasswordRequiresUpper()
    {
        return Error(nameof(PasswordRequiresUpper), "Пароль должен содержать хотя бы одну заглавную букву.");
    }

    public override IdentityError RecoveryCodeRedemptionFailed()
    {
        return Error(nameof(RecoveryCodeRedemptionFailed), "Не удалось использовать резервный код.");
    }

    private static IdentityError Error(string code, string description)
    {
        return new IdentityError
        {
            Code = code,
            Description = description
        };
    }
}
