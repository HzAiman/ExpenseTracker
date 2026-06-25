using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using expense_tracker_web.Models;

namespace expense_tracker_web.Services;

public sealed class ExpenseApiClient(HttpClient httpClient)
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<IReadOnlyList<CategoryResponse>> GetCategoriesAsync()
    {
        return await httpClient.GetFromJsonAsync<IReadOnlyList<CategoryResponse>>(
            "/api/categories",
            JsonOptions) ?? [];
    }

    public async Task<IReadOnlyList<ExpenseResponse>> GetExpensesAsync(
        string? category,
        string? from,
        string? to)
    {
        var query = new StringBuilder("/api/expenses");
        var separator = '?';

        AppendQuery(query, "category", category, ref separator);
        AppendQuery(query, "from", from, ref separator);
        AppendQuery(query, "to", to, ref separator);

        return await httpClient.GetFromJsonAsync<IReadOnlyList<ExpenseResponse>>(
            query.ToString(),
            JsonOptions) ?? [];
    }

    public async Task<ExpenseSummaryResponse> GetSummaryAsync(int? year, int? month)
    {
        var query = new StringBuilder("/api/expenses/summary");
        var separator = '?';

        if (year is not null)
        {
            AppendQuery(query, "year", year.Value.ToString(), ref separator);
        }

        if (month is not null)
        {
            AppendQuery(query, "month", month.Value.ToString(), ref separator);
        }

        using var response = await httpClient.GetAsync(query.ToString());
        return await ReadSuccessAsync<ExpenseSummaryResponse>(response);
    }

    public async Task<ExpenseResponse> CreateExpenseAsync(ExpenseRequest request)
    {
        using var response = await httpClient.PostAsJsonAsync("/api/expenses", request, JsonOptions);
        return await ReadSuccessAsync<ExpenseResponse>(response);
    }

    public async Task<ExpenseResponse> UpdateExpenseAsync(int id, ExpenseRequest request)
    {
        using var response = await httpClient.PutAsJsonAsync($"/api/expenses/{id}", request, JsonOptions);
        return await ReadSuccessAsync<ExpenseResponse>(response);
    }

    public async Task DeleteExpenseAsync(int id)
    {
        using var response = await httpClient.DeleteAsync($"/api/expenses/{id}");
        await EnsureSuccessAsync(response);
    }

    private static void AppendQuery(StringBuilder query, string key, string? value, ref char separator)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return;
        }

        query.Append(separator)
            .Append(Uri.EscapeDataString(key))
            .Append('=')
            .Append(Uri.EscapeDataString(value));

        separator = '&';
    }

    private static async Task<T> ReadSuccessAsync<T>(HttpResponseMessage response)
    {
        await EnsureSuccessAsync(response);

        var value = await response.Content.ReadFromJsonAsync<T>(JsonOptions);
        return value ?? throw new InvalidOperationException("The API returned an empty response.");
    }

    private static async Task EnsureSuccessAsync(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        var message = await ReadErrorMessageAsync(response);
        throw new ExpenseApiException(response.StatusCode, message);
    }

    private static async Task<string> ReadErrorMessageAsync(HttpResponseMessage response)
    {
        try
        {
            using var stream = await response.Content.ReadAsStreamAsync();
            using var document = await JsonDocument.ParseAsync(stream);
            var root = document.RootElement;

            if (root.TryGetProperty("errors", out var errors))
            {
                var messages = errors.EnumerateObject()
                    .SelectMany(property => property.Value.EnumerateArray())
                    .Select(error => error.GetString())
                    .Where(error => !string.IsNullOrWhiteSpace(error));

                return string.Join(" ", messages);
            }

            if (root.TryGetProperty("detail", out var detail))
            {
                return detail.GetString() ?? "The API returned an error.";
            }

            if (root.TryGetProperty("title", out var title))
            {
                return title.GetString() ?? "The API returned an error.";
            }
        }
        catch
        {
            return "The API returned an error.";
        }

        return "The API returned an error.";
    }
}

public sealed class ExpenseApiException(HttpStatusCode statusCode, string message) : Exception(message)
{
    public HttpStatusCode StatusCode { get; } = statusCode;
}
