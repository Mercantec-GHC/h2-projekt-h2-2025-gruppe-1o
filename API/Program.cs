using API.Data;
using API.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Scalar.AspNetCore;
using Serilog;
using System.Reflection;
using System.Text;

// Opsæt en bootstrap-logger for at fange fejl under selve applikationens opstart
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    // Erstat standard-loggeren med Serilog og konfigurer den til at læse fra appsettings.json
    builder.Host.UseSerilog((context, configuration) =>
        configuration.ReadFrom.Configuration(context.Configuration));

    // Registrer de services, der er nødvendige for at bygge standardiserede ProblemDetails-fejlresponser
    builder.Services.AddProblemDetails();

    IConfiguration Configuration = builder.Configuration;

    string connectionString = Configuration.GetConnectionString("DefaultConnection")
    ?? Environment.GetEnvironmentVariable("DefaultConnection");

    builder.Services.AddDbContext<AppDBContext>(options =>
            options.UseNpgsql(connectionString));

    builder.Services.AddMemoryCache();
    builder.Services.AddSingleton<LoginAttemptService>();
    builder.Services.AddScoped<JwtService>();

    // Konfigurer JWT Authentication
    var jwtSecretKey = Configuration["Jwt:SecretKey"] ?? Environment.GetEnvironmentVariable("Jwt:SecretKey") ?? "MyVerySecureSecretKeyThatIsAtLeast32CharactersLong123456789";
    var jwtIssuer = Configuration["Jwt:Issuer"] ?? Environment.GetEnvironmentVariable("Jwt:Issuer") ?? "H2-2025-API";
    var jwtAudience = Configuration["Jwt:Audience"] ?? Environment.GetEnvironmentVariable("Jwt:Audience") ?? "H2-2025-Client";

    builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(jwtSecretKey)),
            ValidateIssuer = true,
            ValidIssuer = jwtIssuer,
            ValidateAudience = true,
            ValidAudience = jwtAudience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

    builder.Services.AddAuthorization();
    builder.Services.AddControllers();

    // Konfigurer Swagger/OpenAPI
    builder.Services.AddSwaggerGen(c =>
    {
        var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
        var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
        if (File.Exists(xmlPath))
        {
            c.IncludeXmlComments(xmlPath);
        }

        c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token in the text input below.",
            Name = "Authorization",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.ApiKey,
            Scheme = "Bearer"
        });

        c.AddSecurityRequirement(new OpenApiSecurityRequirement()
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    },
                    Scheme = "oauth2",
                    Name = "Bearer",
                    In = ParameterLocation.Header,
                },
                new List<string>()
            }
        });
    });

    // Konfigurer CORS
    builder.Services.AddCors(options =>
    {
        options.AddPolicy(
            "AllowSpecificOrigins",
            builder =>
            {
                builder
                    .WithOrigins(
                        "http://localhost:5085",
                        "http://localhost:8052",
                        "https://h2.mercantec.tech"
                    )
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .WithExposedHeaders("Content-Disposition");
            }
        );
    });

    // Konfigurer Health Checks
    builder.Services.AddHealthChecks()
        .AddCheck("self", () => Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy(), new[] { "live" });

    var app = builder.Build();

    // ---- Konfiguration af Middleware Pipeline ----

    // Brug Serilog til at logge alle indkommende HTTP-anmodninger
    app.UseSerilogRequestLogging();

    // Middleware til at fange exceptions og returnere en ProblemDetails-respons
    app.UseExceptionHandler();
    // Middleware til at håndtere andre fejl-statuskoder (f.eks. 404 Not Found)
    app.UseStatusCodePages();

    // Gør kun API-dokumentation tilgængelig i Development-mode for øget sikkerhed
    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();
        app.MapScalarApiReference(options =>
        {
            options
                .WithTitle("H2 Hotel Booking API")
                .WithTheme(ScalarTheme.Mars)
                .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);
        });
        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "API v1");
        });
    }

    app.UseCors("AllowSpecificOrigins");

    // Map Health Checks til endpoints
    app.MapHealthChecks("/health");
    app.MapHealthChecks("/alive", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
    {
        Predicate = r => r.Tags.Contains("live")
    });

    // Aktiver Https Redirection
    app.UseHttpsRedirection();

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();

    app.Run();
}
catch (Exception ex)
{
    // Sørg for at logge fatale fejl, der opstår under selve opstarten
    Log.Fatal(ex, "Applikationen kunne ikke starte.");
}
finally
{
    // Sørg for at alle logs bliver skrevet til filen, før applikationen lukker helt ned
    Log.CloseAndFlush();
}