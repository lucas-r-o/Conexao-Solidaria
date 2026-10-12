namespace Models;

public class Doacao
{
        public Guid Id { get; set; } = Guid.NewGuid();
        public decimal Valor { get; set; }
        public DateTime DataDoacao { get; set; } = DateTime.UtcNow;
        public Guid UsuarioId { get; set; }
        public Guid CampanhaId { get; set; }

        public Usuario? Usuario { get; set; }
        public Campanha? Campanha { get; set; }
}