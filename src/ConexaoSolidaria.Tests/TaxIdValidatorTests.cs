using System.ComponentModel.DataAnnotations;
using Models.Dtos;
using Models.Extensions;

namespace ConexaoSolidaria.Tests;

public class TaxIdValidatorTests
{
    [Theory]
    [InlineData("12345678901", false)]
    [InlineData("11111111111", false)]
    [InlineData("98765432101", false)]
    [InlineData("", false)]
    [InlineData("   ", false)]
    [InlineData("123", false)]
    public void Validar_DeveRetornarFalso_ParaCpfsInvalidos(string taxId, bool expected)
    {
        var result = taxId.IsValidTaxId();
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("111.444.777-35", true)]
    [InlineData("98765432100", true)]
    [InlineData("11144477735", true)]
    public void Validar_DeveRetornarVerdadeiro_ParaCpfsValidos(string taxId, bool expected)
    {
        var result = taxId.IsValidTaxId();
        Assert.Equal(expected, result);
    }

    [Fact]
    public void CadastroDto_DeveRejeitarCpfInvalido()
    {
        var cadastro = new RegistrationDto
        {
            Name = "Donor Teste",
            Email = "teste@exemplo.com",
            TaxId = "12345678901",
            PasswordHash = "Senha123!"
        };
        var results = new List<ValidationResult>();

        var isValid = Validator.TryValidateObject(
            cadastro,
            new ValidationContext(cadastro),
            results,
            validateAllProperties: true);

        Assert.False(isValid);
        Assert.Contains(results, result => result.ErrorMessage == "Tax ID is invalid.");
    }

    [Fact]
    public void CadastroDto_DeveAceitarCpfValido()
    {
        var cadastro = new RegistrationDto
        {
            Name = "Donor Teste",
            Email = "teste@exemplo.com",
            TaxId = "11144477735",
            PasswordHash = "Senha123!"
        };
        var results = new List<ValidationResult>();

        var isValid = Validator.TryValidateObject(
            cadastro,
            new ValidationContext(cadastro),
            results,
            validateAllProperties: true);

        Assert.True(isValid);
        Assert.Empty(results);
    }
}
