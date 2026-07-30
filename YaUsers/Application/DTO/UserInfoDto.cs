using YaContracts.Enums;

namespace YaUsers.Application.DTO;

public class UserInfoDto
{
    public Guid Id { get; set; }
    public string Login { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public UserRoles Role { get; set; }
}