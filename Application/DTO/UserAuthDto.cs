using Domain;

namespace Application.DTO;

public class UserAuthDto
{
    public string Login { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public UserRoles? Role { get; set; } = UserRoles.User;
}