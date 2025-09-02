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

        // --- RETTELSE: Hardkodet API URL ---

        // 1. Definer den specifikke API-adresse, vi altid vil bruge.
        var apiBaseAddress = new Uri("https://flyhigh-api.mercantec.tech/");

        // 2. Registrer en ENKELT HttpClient, som bruger den hardkodede API-adresse.
        builder.Services.AddScoped(sp => new HttpClient { BaseAddress = apiBaseAddress });

        // 3. Registrer dine services. De vil automatisk få den HttpClient, vi oprettede ovenfor.
        builder.Services.AddScoped<APIService>();
        builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthenticationStateProvider>();
        builder.Services.AddAuthorizationCore();

        // --- SLUT PÅ RETTELSE ---

        await builder.Build().RunAsync();
    }
}