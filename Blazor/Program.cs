using System.IdentityModel.Tokens.Jwt;
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

        JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();


        // Peger permanent på dit live API
        var apiBaseAddress = new Uri("http://localhost:8072");

        // Opsætning af en enkelt HttpClient, som alle services deler
        builder.Services.AddScoped(sp => new HttpClient { BaseAddress = apiBaseAddress });
        builder.Services.AddScoped<APIService>();
        builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthenticationStateProvider>();
        builder.Services.AddAuthorizationCore();

        var host = builder.Build();

        // Initialize authentication state on startup
        // This ensures the auth header is set if a token exists in sessionStorage
        var authStateProvider = host.Services.GetRequiredService<AuthenticationStateProvider>();
        await authStateProvider.GetAuthenticationStateAsync();

        await host.RunAsync();
    }
}
