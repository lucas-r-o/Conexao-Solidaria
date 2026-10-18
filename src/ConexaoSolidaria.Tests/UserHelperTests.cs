using Infrastructure;

namespace ConexaoSolidaria.Tests;

public class UserHelperTests
{
    [Fact]
    public void IsValidPassword_DeveAceitarSenhaOriginalERejeitarSenhaDiferente()
    {
        const string password = "Senha123!";
        var hashedPassword = UserHelper.HashPassword(password);

        Assert.NotEqual(password, hashedPassword);
        Assert.True(UserHelper.IsValidPassword(password, hashedPassword));
        Assert.False(UserHelper.IsValidPassword("OutraSenha123!", hashedPassword));
    }
}