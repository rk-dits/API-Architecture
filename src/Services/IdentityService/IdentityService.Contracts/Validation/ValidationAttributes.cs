using System.ComponentModel.DataAnnotations;

namespace IdentityService.Contracts.Validation;

/// <summary>
/// Validates that a password meets minimum security requirements
/// </summary>
public sealed class PasswordValidationAttribute : ValidationAttribute
{
    private readonly int _minimumLength;
    private readonly bool _requireUppercase;
    private readonly bool _requireLowercase;
    private readonly bool _requireDigit;
    private readonly bool _requireSpecialChar;

    public PasswordValidationAttribute(
        int minimumLength = 8,
        bool requireUppercase = true,
        bool requireLowercase = true,
        bool requireDigit = true,
        bool requireSpecialChar = true)
    {
        _minimumLength = minimumLength;
        _requireUppercase = requireUppercase;
        _requireLowercase = requireLowercase;
        _requireDigit = requireDigit;
        _requireSpecialChar = requireSpecialChar;
    }

    public int MinimumLength => _minimumLength;
    public bool RequireUppercase => _requireUppercase;
    public bool RequireLowercase => _requireLowercase;
    public bool RequireDigit => _requireDigit;
    public bool RequireSpecialChar => _requireSpecialChar;

    public override bool IsValid(object? value)
    {
        if (value is not string password || string.IsNullOrWhiteSpace(password))
        {
            return false;
        }

        if (password.Length < _minimumLength)
        {
            ErrorMessage = $"Password must be at least {_minimumLength} characters long";
            return false;
        }

        if (_requireUppercase && !password.Any(char.IsUpper))
        {
            ErrorMessage = "Password must contain at least one uppercase letter";
            return false;
        }

        if (_requireLowercase && !password.Any(char.IsLower))
        {
            ErrorMessage = "Password must contain at least one lowercase letter";
            return false;
        }

        if (_requireDigit && !password.Any(char.IsDigit))
        {
            ErrorMessage = "Password must contain at least one digit";
            return false;
        }

        if (_requireSpecialChar && !password.Any(c => !char.IsLetterOrDigit(c)))
        {
            ErrorMessage = "Password must contain at least one special character";
            return false;
        }

        return true;
    }
}

/// <summary>
/// Validates email format
/// </summary>
public sealed class EmailValidationAttribute : ValidationAttribute
{
    public override bool IsValid(object? value)
    {
        if (value is not string email || string.IsNullOrWhiteSpace(email))
        {
            return false;
        }

        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            ErrorMessage = "Invalid email format";
            return false;
        }
    }
}

/// <summary>
/// Validates username format (alphanumeric and specific special characters)
/// </summary>
public sealed class UsernameValidationAttribute : ValidationAttribute
{
    private readonly int _minimumLength;
    private readonly int _maximumLength;

    public UsernameValidationAttribute(int minimumLength = 3, int maximumLength = 50)
    {
        _minimumLength = minimumLength;
        _maximumLength = maximumLength;
    }

    public int MinimumLength => _minimumLength;
    public int MaximumLength => _maximumLength;

    public override bool IsValid(object? value)
    {
        if (value is not string username || string.IsNullOrWhiteSpace(username))
        {
            return false;
        }

        if (username.Length < _minimumLength || username.Length > _maximumLength)
        {
            ErrorMessage = $"Username must be between {_minimumLength} and {_maximumLength} characters";
            return false;
        }

        // Allow alphanumeric characters, dots, hyphens, and underscores
        if (!username.All(c => char.IsLetterOrDigit(c) || c == '.' || c == '-' || c == '_'))
        {
            ErrorMessage = "Username can only contain letters, numbers, dots, hyphens, and underscores";
            return false;
        }

        // Cannot start or end with special characters
        if (!char.IsLetterOrDigit(username[0]) || !char.IsLetterOrDigit(username[^1]))
        {
            ErrorMessage = "Username must start and end with a letter or number";
            return false;
        }

        return true;
    }
}