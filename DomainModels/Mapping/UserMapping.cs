namespace DomainModels.Mapping;

/// <summary>
/// En statisk hjælpeklasse til at mappe mellem forskellige bruger-objekter.
/// </summary>
public class UserMapping
{
    /// <summary>
    /// Konverterer et User-databaseobjekt til et UserGetDto, der er sikkert at sende til en klient.
    /// </summary>
    /// <param name="user">User-entiteten fra databasen.</param>
    /// <returns>Et UserGetDto-objekt.</returns>
    public static UserGetDto ToUserGetDto(User user)
    {
        return new UserGetDto
        {
            Id = user.Id,
            Email = user.Email,
            Username = user.Username,
            Role = user.Role?.Name ?? string.Empty
        };
    }
}