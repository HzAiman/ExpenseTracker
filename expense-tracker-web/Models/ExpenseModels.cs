namespace expense_tracker_web.Models;

public sealed record ExpenseResponse(
    int Id,
    decimal Amount,
    int CategoryId,
    string Category,
    string Description,
    DateOnly Date);

public sealed record ExpenseRequest(
    decimal Amount,
    int CategoryId,
    string Description,
    string Date);

public sealed record CategoryResponse(int Id, string Name);

public sealed record ExpenseSummaryResponse(
    int Year,
    int? Month,
    string Period,
    DateOnly From,
    DateOnly To,
    decimal Total,
    IReadOnlyList<MonthlyTotalResponse> Months,
    IReadOnlyList<CategorySummaryResponse> Categories);

public sealed record MonthlyTotalResponse(
    int Month,
    decimal Total,
    int ExpenseCount);

public sealed record CategorySummaryResponse(
    int CategoryId,
    string Category,
    decimal Total,
    int ExpenseCount);
