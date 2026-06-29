namespace PraeferenzRoO.Domain.Enums;

/// <summary>
/// RBAC roles permitted in the system.
/// Only these three roles are authorised — no other role may be added without written approval.
/// </summary>
public enum UserRole
{
    Admin = 1,
    Operator = 2,
    Viewer = 3
}
