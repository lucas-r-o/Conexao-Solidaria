namespace Models;

public class Donation
{
        public Guid Id { get; set; } = Guid.NewGuid();
        public decimal Amount { get; set; }
        public DateTime DonationDate { get; set; } = DateTime.UtcNow;
        public Guid UserId { get; set; }
        public Guid CampaignId { get; set; }

        public User? User { get; set; }
        public Campaign? Campaign { get; set; }
}