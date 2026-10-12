namespace Models;

public class Campanha
{
    public Guid Id { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public DateTime DataInicio { get; set; } = DateTime.UtcNow;
    public DateTime? DataFim { get; set; }
    public decimal MetaFinanceira { get; set; }
    public decimal ValorArrecadado { get; set; }
    public Status Status { get; set; } = Status.Ativo;

    public ICollection<Doacao> Doacoes { get; set; } = [];
}