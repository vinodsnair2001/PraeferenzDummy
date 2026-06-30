using PraeferenzRoO.Domain.Common;
using PraeferenzRoO.Domain.Enums;

namespace PraeferenzRoO.Domain.Entities;

/// <summary>
/// Represents a system user authenticated via username/password.
/// Stores only the BCrypt hash of the password and the BCrypt hash of the refresh token —
/// the raw values are never persisted.
/// </summary>
public class User : AggregateRoot
{
    /// <summary>Gets or sets the tenant this user belongs to.</summary>
    public Guid TenantId { get; set; }

    /// <summary>Gets or sets the unique login name for this user.</summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>Gets or sets the email address for this user.</summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>Gets or sets the BCrypt hash of the user's password.</summary>
    public string PasswordHash { get; set; } = string.Empty;

    /// <summary>Gets or sets the BCrypt hash of the user's current refresh token. Never store the raw token.</summary>
    public string? RefreshTokenHash { get; set; }

    /// <summary>Gets or sets the UTC expiry date/time of the current refresh token.</summary>
    public DateTime? RefreshTokenExpiryDate { get; set; }

    /// <summary>Gets or sets the RBAC role assigned to this user.</summary>
    public UserRole Role { get; set; }

    /// <summary>Gets or sets a value indicating whether this user account is active.</summary>
    public bool IsActive { get; set; }
}
