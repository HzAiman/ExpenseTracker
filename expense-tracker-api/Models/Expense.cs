namespace ExpenseTrackerApi.Models;

public sealed class Expense
{
    public int Id { get; set; }

    public decimal Amount { get; set; }

    public int CategoryId { get; set; }

    public Category Category { get; set; } = null!;

    public string Description { get; set; } = string.Empty;

    public DateOnly Date { get; set; }
}
