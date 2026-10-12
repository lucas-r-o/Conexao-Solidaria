namespace Models.Dtos
{
    public class DoacaoDto
    {
        public Guid CampanhaId { get; set; }
        public Guid DoadorId { get; set; }
        public decimal Valor { get; set; }
    }
}
