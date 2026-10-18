namespace Infrastructure
{
    public class UserHelper
    {
        public static string HashPassword(string password) => BCrypt.Net.BCrypt.HashPassword(password);
        public static bool IsValidPassword(string sentPassword, string userPassword) => BCrypt.Net.BCrypt.Verify(sentPassword, userPassword);
    }
}
