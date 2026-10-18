using System.ComponentModel.DataAnnotations;

namespace Models.DTOs;

public class UpdateCampaignDto
{
    [Required(ErrorMessage = "Título é obrigatório.")]
    [MaxLength(100, ErrorMessage = "Título deve ter no máximo 100 caracteres.")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Descrição é obrigatória.")]
    [MaxLength(500, ErrorMessage = "Descrição deve ter no máximo 500 caracteres.")]
    public string Description { get; set; } = string.Empty;

    [Range(0.01, double.MaxValue, ErrorMessage = "Meta financeira deve ser maior que zero.")]
    public decimal FinancialGoal { get; set; }

    [Required(ErrorMessage = "Status é obrigatório.")]
    public Status Status { get; set; }
    [EndDateValidator]
    public DateTime? EndDate { get; set; }
}
