using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using expense_tracker_web;
using expense_tracker_web.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
var apiBaseUrl = builder.Configuration["ApiBaseUrl"]
    ?? throw new InvalidOperationException("ApiBaseUrl is missing from Blazor configuration.");

builder.Services.AddScoped(sp => new ExpenseApiClient(new HttpClient
{
    BaseAddress = new Uri(apiBaseUrl)
}));

await builder.Build().RunAsync();
