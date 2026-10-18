namespace Models;

public class Campaign
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime StartDate { get; set; } = DateTime.UtcNow;
    public DateTime? EndDate { get; set; }
    public decimal FinancialGoal { get; set; }
    public decimal AmountRaised { get; set; }
    public Status Status { get; set; } = Status.Active;

    public ICollection<Donation> Donations { get; set; } = [];
}