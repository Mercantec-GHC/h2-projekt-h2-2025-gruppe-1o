namespace DomainModels.Mapping;

public class UserMapping
{
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
