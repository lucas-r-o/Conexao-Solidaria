using System.ComponentModel.DataAnnotations;
using Models.DTOs;

namespace ConexaoSolidaria.Tests;

public class DataFimValidatorTests
{
    [Fact]
    public void CriarCampanhaDto_DeveRejeitarDataFimAnteriorAoDiaAtualUtc()
    {
        var errors = Validate(DateTime.UtcNow.Date.AddDays(-1));

        Assert.Contains(errors, error =>
            error.ErrorMessage == "Data Tem que ser superior a data atual.");
    }

    [Fact]
    public void CriarCampanhaDto_DeveAceitarDataFimFutura()
    {
        var errors = Validate(DateTime.UtcNow.Date.AddDays(1));

        Assert.Empty(errors);
    }

    [Fact]
    public void CriarCampanhaDto_DeveAceitarDataFimNula()
    {
        var errors = Validate(null);

        Assert.Empty(errors);
    }

    private static List<ValidationResult> Validate(DateTime? dataFim)
    {
        var campanha = new CriarCampanhaDTO
        {
            Titulo = "Campanha de teste",
            Descricao = "Descrição de teste",
            MetaFinanceira = 100m,
            DataFim = dataFim
        };
        var errors = new List<ValidationResult>();

        Validator.TryValidateObject(
            campanha,
            new ValidationContext(campanha),
            errors,
            validateAllProperties: true);

        return errors;
    }
}