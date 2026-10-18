namespace Models;

public class User
{
    // TODO: Considerar mapeamento para quando subir a migration, considerar gerar o guid direto pelo banco
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string TaxId { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public Roles Role { get; set; } = Roles.Donor;

    public ICollection<Donation> Donations { get; set; } = [];
}