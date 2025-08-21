using API.Data;
using API.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
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

    // Erstat standard-loggeren med Serilog
    builder.Host.UseSerilog((context, configuration) =>
        configuration.ReadFrom.Configuration(context.Configuration));

    builder.Services.AddProblemDetails();

    IConfiguration Configuration = builder.Configuration;
    string connectionString = Configuration.GetConnectionString("DefaultConnection")
        ?? Environment.GetEnvironmentVariable("DefaultConnection");

    builder.Services.AddDbContext<AppDBContext>(options =>
        options.UseNpgsql(connectionString));

    // Service registrering
    builder.Services.AddMemoryCache();
    builder.Services.AddSingleton<LoginAttemptService>();
    builder.Services.AddScoped<JwtService>();
    builder.Services.AddScoped<DataSeederService>();

    // JWT Authentication
    var jwtSecretKey = Configuration["Jwt:SecretKey"] ?? "MyVerySecureSecretKeyThatIsAtLeast32CharactersLong123456789";
    var jwtIssuer = Configuration["Jwt:Issuer"] ?? "H2-2025-API";
    var jwtAudience = Configuration["Jwt:Audience"] ?? "H2-2025-Client";

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

    // Swagger/OpenAPI
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo { Title = "H2 Hotel Booking API", Version = "v1" });
        var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
        var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
        if (File.Exists(xmlPath))
        {
            c.IncludeXmlComments(xmlPath);
        }
        c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Description = "JWT Authorization. Skriv 'Bearer' [space] og token.",
            Name = "Authorization",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.ApiKey,
            Scheme = "Bearer"
        });
        c.AddSecurityRequirement(new OpenApiSecurityRequirement {
        {
            new OpenApiSecurityScheme {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            new string[] {}
        }});
    });

    // CORS
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowSpecificOrigins", corsBuilder =>
        {
            corsBuilder.WithOrigins("http://localhost:5085", "http://localhost:8052", "https://h2.mercantec.tech")
                   .AllowAnyMethod()
                   .AllowAnyHeader()
                   .WithExposedHeaders("Content-Disposition");
        });
    });

    var app = builder.Build();

    // ---- Middleware Pipeline ----

    // KORREKT FEJLHÅNDTERING: Viser detaljerede fejl i Development, og en generisk side i Production.
    if (app.Environment.IsDevelopment())
    {
        app.UseDeveloperExceptionPage(); // <--- DENNE VISER DEN RIGTIGE FEJL
        app.UseSwagger();
        app.UseSwaggerUI(options => options.SwaggerEndpoint("/swagger/v1/swagger.json", "API v1"));
    }
    else
    {
        app.UseExceptionHandler("/error"); // <--- Denne bruges kun i produktion
        app.UseHsts();
    }

    app.UseHttpsRedirection();
    app.UseSerilogRequestLogging();
    app.UseCors("AllowSpecificOrigins");
    app.UseAuthentication();
    app.UseAuthorization();
    app.MapControllers();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Applikationen kunne ikke starte.");
}
finally
{
    Log.CloseAndFlush();
}