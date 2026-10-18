namespace Models.Dtos
{
    public class DonationDto
    {
        public Guid CampaignId { get; set; }
        public Guid DonorId { get; set; }
        public decimal Amount { get; set; }
    }
}
