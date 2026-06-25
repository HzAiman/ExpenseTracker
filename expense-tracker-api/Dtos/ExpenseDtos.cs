using System.ComponentModel.DataAnnotations;

namespace ExpenseTrackerApi.Dtos;

public sealed record ExpenseResponse(
    int Id,
    decimal Amount,
    int CategoryId,
    string Category,
    string Description,
    DateOnly Date);

public sealed record CreateExpenseRequest(
    [Range(0.01, 1_000_000)] decimal Amount,
    [Range(1, int.MaxValue)] int CategoryId,
    [Required, MaxLength(240)] string Description,
    DateOnly Date);

public sealed record UpdateExpenseRequest(
    [Range(0.01, 1_000_000)] decimal Amount,
    [Range(1, int.MaxValue)] int CategoryId,
    [Required, MaxLength(240)] string Description,
    DateOnly Date);

public sealed record CategoryResponse(int Id, string Name);

public sealed record ExpenseSummaryResponse(
    int Year,
    int? Month,
    string Period,
    DateOnly From,
    DateOnly To,
    decimal Total,
    IReadOnlyCollection<MonthlyTotalResponse> Months,
    IReadOnlyCollection<CategorySummaryResponse> Categories);

public sealed record MonthlyTotalResponse(
    int Month,
    decimal Total,
    int ExpenseCount);

public sealed record CategorySummaryResponse(
    int CategoryId,
    string Category,
    decimal Total,
    int ExpenseCount);
