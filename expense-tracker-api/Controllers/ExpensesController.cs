using ExpenseTrackerApi.Data;
using ExpenseTrackerApi.Dtos;
using ExpenseTrackerApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTrackerApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class ExpensesController(
    ExpenseTrackerDbContext dbContext,
    ILogger<ExpensesController> logger) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<IReadOnlyCollection<ExpenseResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IReadOnlyCollection<ExpenseResponse>>> GetExpenses(
        [FromQuery] string? category,
        [FromQuery] DateOnly? from,
        [FromQuery] DateOnly? to,
        CancellationToken cancellationToken)
    {
        if (from is not null && to is not null && from > to)
        {
            ModelState.AddModelError(nameof(from), "'from' must be earlier than or equal to 'to'.");
            return ValidationProblem(ModelState);
        }

        var query = dbContext.Expenses
            .AsNoTracking()
            .Include(expense => expense.Category)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(category))
        {
            query = query.Where(expense => expense.Category.Name == category);
        }

        if (from is not null)
        {
            query = query.Where(expense => expense.Date >= from);
        }

        if (to is not null)
        {
            query = query.Where(expense => expense.Date <= to);
        }

        var expenses = await query
            .OrderByDescending(expense => expense.Date)
            .ThenByDescending(expense => expense.Id)
            .Select(expense => ToResponse(expense))
            .ToListAsync(cancellationToken);

        return Ok(expenses);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType<ExpenseResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ExpenseResponse>> GetExpense(
        int id,
        CancellationToken cancellationToken)
    {
        var expense = await dbContext.Expenses
            .AsNoTracking()
            .Include(expense => expense.Category)
            .FirstOrDefaultAsync(expense => expense.Id == id, cancellationToken);

        return expense is null ? NotFound() : Ok(ToResponse(expense));
    }

    [HttpGet("summary")]
    [ProducesResponseType<ExpenseSummaryResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ExpenseSummaryResponse>> GetSummary(
        [FromQuery] int? year,
        [FromQuery] int? month,
        CancellationToken cancellationToken)
    {
        if (year is null)
        {
            ModelState.AddModelError(nameof(year), "'year' is required.");
        }
        else if (year is < 1 or > 9999)
        {
            ModelState.AddModelError(nameof(year), "'year' must be between 1 and 9999.");
        }

        if (month is < 1 or > 12)
        {
            ModelState.AddModelError(nameof(month), "'month' must be between 1 and 12.");
        }

        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var requestedYear = year.GetValueOrDefault();
        var start = month is null
            ? new DateOnly(requestedYear, 1, 1)
            : new DateOnly(requestedYear, month.Value, 1);
        var exclusiveEnd = month is null
            ? start.AddYears(1)
            : start.AddMonths(1);
        var inclusiveEnd = exclusiveEnd.AddDays(-1);

        var baseQuery = dbContext.Expenses
            .AsNoTracking()
            .Where(expense => expense.Date >= start && expense.Date < exclusiveEnd);

        var categoryTotals = await baseQuery
            .GroupBy(expense => new { expense.CategoryId, expense.Category.Name })
            .Select(group => new
            {
                group.Key.CategoryId,
                Category = group.Key.Name,
                Total = group.Sum(expense => expense.Amount),
                ExpenseCount = group.Count()
            })
            .ToListAsync(cancellationToken);

        var categories = categoryTotals
            .Select(summary => new CategorySummaryResponse(
                summary.CategoryId,
                summary.Category,
                summary.Total,
                summary.ExpenseCount))
            .OrderByDescending(summary => summary.Total)
            .ToList();

        var monthlyTotals = await baseQuery
            .GroupBy(expense => expense.Date.Month)
            .Select(group => new
            {
                Month = group.Key,
                Total = group.Sum(expense => expense.Amount),
                ExpenseCount = group.Count()
            })
            .ToListAsync(cancellationToken);

        var months = Enumerable.Range(month ?? 1, month is null ? 12 : 1)
            .GroupJoin(
                monthlyTotals,
                monthNumber => monthNumber,
                summary => summary.Month,
                (monthNumber, matchingSummaries) =>
                {
                    var summary = matchingSummaries.SingleOrDefault();
                    return new MonthlyTotalResponse(
                        monthNumber,
                        summary?.Total ?? 0,
                        summary?.ExpenseCount ?? 0);
                })
            .ToList();

        return Ok(new ExpenseSummaryResponse(
            requestedYear,
            month,
            month is null ? "year" : "month",
            start,
            inclusiveEnd,
            categories.Sum(summary => summary.Total),
            months,
            categories));
    }

    [HttpPost]
    [ProducesResponseType<ExpenseResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ExpenseResponse>> CreateExpense(
        CreateExpenseRequest request,
        CancellationToken cancellationToken)
    {
        if (!await CategoryExists(request.CategoryId, cancellationToken))
        {
            ModelState.AddModelError(nameof(request.CategoryId), "Category does not exist.");
            return ValidationProblem(ModelState);
        }

        var expense = new Expense
        {
            Amount = request.Amount,
            CategoryId = request.CategoryId,
            Description = request.Description.Trim(),
            Date = request.Date
        };

        dbContext.Expenses.Add(expense);
        await dbContext.SaveChangesAsync(cancellationToken);

        logger.LogInformation(
            "Created expense {ExpenseId} in category {CategoryId} for {Amount}",
            expense.Id,
            expense.CategoryId,
            expense.Amount);

        await dbContext.Entry(expense).Reference(item => item.Category).LoadAsync(cancellationToken);

        return CreatedAtAction(nameof(GetExpense), new { id = expense.Id }, ToResponse(expense));
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType<ExpenseResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ExpenseResponse>> UpdateExpense(
        int id,
        UpdateExpenseRequest request,
        CancellationToken cancellationToken)
    {
        var expense = await dbContext.Expenses
            .Include(expense => expense.Category)
            .FirstOrDefaultAsync(expense => expense.Id == id, cancellationToken);

        if (expense is null)
        {
            return NotFound();
        }

        if (!await CategoryExists(request.CategoryId, cancellationToken))
        {
            ModelState.AddModelError(nameof(request.CategoryId), "Category does not exist.");
            return ValidationProblem(ModelState);
        }

        expense.Amount = request.Amount;
        expense.CategoryId = request.CategoryId;
        expense.Description = request.Description.Trim();
        expense.Date = request.Date;

        await dbContext.SaveChangesAsync(cancellationToken);
        await dbContext.Entry(expense).Reference(item => item.Category).LoadAsync(cancellationToken);

        logger.LogInformation("Updated expense {ExpenseId}", expense.Id);

        return Ok(ToResponse(expense));
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteExpense(int id, CancellationToken cancellationToken)
    {
        var expense = await dbContext.Expenses.FindAsync([id], cancellationToken);

        if (expense is null)
        {
            return NotFound();
        }

        dbContext.Expenses.Remove(expense);
        await dbContext.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Deleted expense {ExpenseId}", id);

        return NoContent();
    }

    private async Task<bool> CategoryExists(int categoryId, CancellationToken cancellationToken)
    {
        return await dbContext.Categories.AnyAsync(
            category => category.Id == categoryId,
            cancellationToken);
    }

    private static ExpenseResponse ToResponse(Expense expense)
    {
        return new ExpenseResponse(
            expense.Id,
            expense.Amount,
            expense.CategoryId,
            expense.Category.Name,
            expense.Description,
            expense.Date);
    }
}
