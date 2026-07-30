using YaUsers.Domain;

namespace YaUsers.Application.Repositories;

public interface IUserRepository
{
    public Task<User?> GetUserByIdAsync(Guid id, CancellationToken cancellationToken = default);

    public Task<User?> GetUserByLoginAsync(string login, CancellationToken cancellationToken = default);

    public Task AddUserAsync(User user, CancellationToken cancellationToken = default);

    public Task SaveChangesAsync(CancellationToken cancellationToken = default);
}