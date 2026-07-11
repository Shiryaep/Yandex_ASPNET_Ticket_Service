using Application.DTO;
using YaContracts.Enums;

namespace Application.Services.UserServices;

public interface IUserService
{
    public Task<UserInfoDto> RegisterUserAsync(string login, string password, UserRoles? userRole = UserRoles.User, CancellationToken cancellationToken = default);

    public Task<UserInfoDto> SignInUserAsync(string login, string password, CancellationToken cancellationToken = default);

    public Task<bool> PromoteToAdmin(Guid id, CancellationToken cancellationToken = default);

    public Task<UserInfoDto> GetUserByIdAsync(Guid id, CancellationToken cancellationToken = default);

    public Task<UserInfoDto> GetUserByLoginAsync(string login, CancellationToken cancellationToken = default);
}