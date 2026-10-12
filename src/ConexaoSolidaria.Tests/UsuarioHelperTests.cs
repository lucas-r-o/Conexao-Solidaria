using Infrastructure;

namespace ConexaoSolidaria.Tests;

public class UsuarioHelperTests
{
    [Fact]
    public void IsValidPassword_DeveAceitarSenhaOriginalERejeitarSenhaDiferente()
    {
        const string password = "Senha123!";
        var hashedPassword = UsuarioHelper.HashPassword(password);

        Assert.NotEqual(password, hashedPassword);
        Assert.True(UsuarioHelper.IsValidPassword(password, hashedPassword));
        Assert.False(UsuarioHelper.IsValidPassword("OutraSenha123!", hashedPassword));
    }
}