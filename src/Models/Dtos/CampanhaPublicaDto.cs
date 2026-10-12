namespace Models.DTOs;

public class CampanhaPublicaDTO
{
    public Guid Id { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public decimal MetaFinanceira { get; set; }
    public decimal ValorTotalArrecadado { get; set; }
}
