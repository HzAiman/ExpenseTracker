using ExpenseTrackerApi.Data;
using ExpenseTrackerApi.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTrackerApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class CategoriesController(ExpenseTrackerDbContext dbContext) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<IReadOnlyCollection<CategoryResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyCollection<CategoryResponse>>> GetCategories(
        CancellationToken cancellationToken)
    {
        var categories = await dbContext.Categories
            .AsNoTracking()
            .OrderBy(category => category.Name)
            .Select(category => new CategoryResponse(category.Id, category.Name))
            .ToListAsync(cancellationToken);

        return Ok(categories);
    }
}
