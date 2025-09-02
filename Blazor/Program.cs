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

        // 1. Definer API endpoint (her kan du senere bruge appsettings.json, hvis du vil)
        var apiEndpoint = "https://flyhigh-api.mercantec.tech/";

        // 2. Registrer en standard HttpClient som Scoped. Alle services, der beder om en HttpClient, får nu DEN SAMME.
        builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(apiEndpoint) });

        // 3. Registrer APIService og AuthenticationStateProvider som Scoped.
        //    De vil begge modtage den HttpClient, vi registrerede ovenfor.
        builder.Services.AddScoped<APIService>();
        builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthenticationStateProvider>();

        // 4. Tilføj Authorization Core services
        builder.Services.AddAuthorizationCore();

        await builder.Build().RunAsync();
    }
}