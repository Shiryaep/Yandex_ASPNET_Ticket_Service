using YaUsers.Application.DTO;

namespace YaUsers.Application.Services
{
    public interface IJWTService
    {
        public string GenerateToken(UserInfoDto user);
    }
}
