using System.ComponentModel.DataAnnotations;

namespace Models.DTOs;

public class CriarCampanhaDTO
{
    [Required(ErrorMessage = "Título é obrigatório.")]
    [MaxLength(100, ErrorMessage = "Título deve ter no máximo 100 caracteres.")]
    public string Titulo { get; set; } = string.Empty;
    [Required(ErrorMessage = "Descrição é obrigatória.")]
    [MaxLength(500, ErrorMessage = "Descrição deve ter no máximo 500 caracteres.")]
    public string Descricao { get; set; } = string.Empty;
    [Range(0.01, double.MaxValue, ErrorMessage = "Meta financeira deve ser maior que zero.")]
    public decimal MetaFinanceira { get; set; }
    [DataFimValidator]
    public DateTime? DataFim { get; set; }
}