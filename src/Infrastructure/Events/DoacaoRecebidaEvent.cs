namespace ConexaoSolidaria.Events
{
    public class DoacaoRecebidaEvent
    {
        public Guid Id { get; set; }
        public Guid CampanhaId { get; set; }
        public Guid DoadorId { get; set; }
        public decimal Valor { get; set; }
        public DateTime DataHora { get; set; } = DateTime.Now;
        public decimal MetaFinanceira { get; set; }
    }
}
