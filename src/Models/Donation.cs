namespace Models;

public class Donation
{
        public Guid Id { get; set; } = Guid.NewGuid();
        public decimal Valor { get; set; }
        public DateTime DataDoacao { get; set; } = DateTime.UtcNow;
        public Guid UsuarioId { get; set; }
        public Guid CampanhaId { get; set; }

        public User? Usuario { get; set; }
        public Campaign? Campanha { get; set; }
}