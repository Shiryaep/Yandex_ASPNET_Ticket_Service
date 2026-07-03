using Application.Repositories;
using Domain;
using Infrastructure.DataAccess;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class UserRepository(AppDbContext db) : IUserRepository
    {
        private readonly AppDbContext _db = db;

        public Task AddUserAsync(User user, CancellationToken cancellationToken = default)
        {
            return _db.Users.AddAsync(user, cancellationToken).AsTask();
        }

        public Task<User?> GetUserByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return _db.Users.FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
        }

        public Task<User?> GetUserByLoginAsync(string login, CancellationToken cancellationToken = default)
        {
            return _db.Users.FirstOrDefaultAsync(u => u.Login == login, cancellationToken);
        }

        public Task SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return _db.SaveChangesAsync(cancellationToken);
        }
    }
}
