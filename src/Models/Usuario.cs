namespace Models;

public class Usuario
{
    // TODO: Considerar mapeamento para quando subir a migration, considerar gerar o guid direto pelo banco
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string CPF { get; set; } = string.Empty;
    public string SenhaHash { get; set; } = string.Empty;
    public Roles Role { get; set; } = Roles.Doador;

    public ICollection<Doacao> Doacoes { get; set; } = [];
}