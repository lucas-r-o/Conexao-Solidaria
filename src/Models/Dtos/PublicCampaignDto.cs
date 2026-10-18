namespace Models.DTOs;

public class PublicCampaignDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public decimal FinancialGoal { get; set; }
    public decimal TotalAmountRaised { get; set; }
}
