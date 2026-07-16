using System.Security.Cryptography;
using System.Text;

namespace YaUsers.Application.Services
{
    public static class PasswordService
    {
        public static string GetHash(string password)
        {
            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(password));
            return Convert.ToHexString(bytes);
        }

        public static bool IsValid(string password, string hash)
        {
            return hash == GetHash(password);
        }
    }
}
