using Blazor.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

namespace Blazor;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebAssemblyHostBuilder.CreateDefault(args);
        builder.RootComponents.Add<App>("#app");
        builder.RootComponents.Add<HeadOutlet>("head::after");

        // Hardkodet API-adresse
        var apiBaseAddress = new Uri("https://flyhigh-api.mercantec.tech/");

        // Opsætning af en enkelt HttpClient, som alle services deler
        builder.Services.AddScoped(sp => new HttpClient { BaseAddress = apiBaseAddress });

        builder.Services.AddScoped<APIService>();
        builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthenticationStateProvider>();
        builder.Services.AddAuthorizationCore();

        await builder.Build().RunAsync();
    }
}