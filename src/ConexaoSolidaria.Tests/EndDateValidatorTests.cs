using System.ComponentModel.DataAnnotations;
using Models.DTOs;

namespace ConexaoSolidaria.Tests;

public class EndDateValidatorTests
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

    private static List<ValidationResult> Validate(DateTime? endDate)
    {
        var campaign = new CreateCampaignDto
        {
            Title = "Campaign de teste",
            Description = "Descrição de teste",
            FinancialGoal = 100m,
            EndDate = endDate
        };
        var errors = new List<ValidationResult>();

        Validator.TryValidateObject(
            campaign,
            new ValidationContext(campaign),
            errors,
            validateAllProperties: true);

        return errors;
    }
}