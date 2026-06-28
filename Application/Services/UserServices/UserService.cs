using Application.DTO;
using Application.Repositories;
using Domain;
using Domain.Exceptions;

namespace Application.Services.UserServices;

public class UserService(IUserRepository userRepository) : IUserService
{
    private readonly IUserRepository _userRepository = userRepository;

    public async Task<UserInfoDto> RegisterUserAsync(string login, string password, UserRoles? userRole = UserRoles.User, CancellationToken cancellationToken = default)
    {
        User user = User.Create(login, PasswordService.GetHash(password), userRole);
        await _userRepository.AddUserAsync(user, cancellationToken);
        await _userRepository.SaveChangesAsync(cancellationToken);
        return ToInfo(user);
    }

    public async Task<UserInfoDto> SignInUserAsync(string login, string password, CancellationToken cancellationToken = default)
    {
        User user = await _userRepository.GetUserByLoginAsync(login, cancellationToken)
            ?? throw new NotFoundException("User not found");

        if (PasswordService.GetHash(password) != user.PasswordHash)
            throw new ValidationException("Wrong password");

        return ToInfo(user);
    }

    public async Task<bool> PromoteToAdmin(Guid id, CancellationToken cancellationToken = default)
    {
        User user = await _userRepository.GetUserByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("User not found");

        user.PromoteToAdmin();
        await _userRepository.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<UserInfoDto> GetUserByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        User user = await _userRepository.GetUserByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("User not found");

        return ToInfo(user);
    }

    public async Task<UserInfoDto> GetUserByLoginAsync(string login, CancellationToken cancellationToken = default)
    {
        User user = await _userRepository.GetUserByLoginAsync(login, cancellationToken)
            ?? throw new NotFoundException("User not found");

        return ToInfo(user);
    }

    public static UserInfoDto ToInfo(User user) => new()
    {
        Id = user.Id,
        Login = user.Login,
        PasswordHash = user.PasswordHash,
        Role = user.Role
    };
}