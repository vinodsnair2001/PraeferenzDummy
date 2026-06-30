using UUIDNext;

namespace PraeferenzRoO.Shared.Helpers;

public static class UuidGenerator
{
    public static Guid NewId() => Uuid.NewDatabaseFriendly(Database.PostgreSql);
}
