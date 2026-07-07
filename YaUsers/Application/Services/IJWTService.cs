using Application.DTO;

namespace Application.Services.UserServices
{
    public interface IJWTService
    {
        public string GenerateToken(UserInfoDto user);
    }
}
