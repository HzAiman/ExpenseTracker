namespace ExpenseTrackerApi.Models;

public sealed class Category
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public ICollection<Expense> Expenses { get; set; } = [];
}
