using Domain.Exceptions;

namespace Domain;

/// <summary> User entity </summary> 
public class User
{
    public Guid Id { get; set; }
    public string Login { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public UserRoles Role { get; set; }
    public List<Booking> Bookings { get; set; } = [];

    private User() //Only for EF
    {
    }

    private User(Guid id, string login, string passwordHash, UserRoles role)
    {
        Id = id;
        Login = login;
        PasswordHash = passwordHash;
        Role = role;
    }

    public static User Create(string? login, string? passwordHash, UserRoles? role)
    {
        ValidateModelFields(login, passwordHash, role);
        return new User(Guid.NewGuid(), login!, passwordHash!, role!.Value);
    }

    public void PromoteToAdmin()
    {
        Role = UserRoles.Admin;
    }

    private static void ValidateModelFields(string? login, string? password, UserRoles? role)
    {
        if (string.IsNullOrWhiteSpace(login))
            throw new ValidationException("There is no Login!");

        if (string.IsNullOrWhiteSpace(password))
            throw new ValidationException("There is no Password!");

        if (!role.HasValue)
            throw new ValidationException("There is no Role!");
    }
}