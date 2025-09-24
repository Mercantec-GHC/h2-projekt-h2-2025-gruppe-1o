using API.Data;
using API.Hubs;
using API.Repositories;
using API.Services;
using DomainModels.Settings;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, configuration) =>
    configuration.ReadFrom.Configuration(context.Configuration));

IConfiguration Configuration = builder.Configuration;
string connectionString = Configuration.GetConnectionString("DefaultConnection")!;
builder.Services.AddDbContext<AppDBContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddMemoryCache();
builder.Services.AddSingleton<LoginAttemptService>();
builder.Services.AddScoped<JwtService>();
builder.Services.AddScoped<DataSeederService>();

// ----- OPDATERET TIL SENDGRID -----
builder.Services.Configure<SendGridSettings>(builder.Configuration.GetSection("SendGridSettings"));
builder.Services.AddScoped<MailService>();
// ----------------------------------

builder.Services.AddScoped<IBookingRepository, BookingRepository>();
builder.Services.AddScoped<ITicketRepository, TicketRepository>();
builder.Services.AddScoped<ActiveDirectoryTesting.ActiveDirectoryService>();
builder.Services.AddSignalR();
builder.Services.AddCors();
builder.Services.AddRouting();

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
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];
            var path = context.HttpContext.Request.Path;
            if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/ticketHub"))
            {
                context.Token = accessToken;
            }
            return Task.CompletedTask;
        }
    };
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

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Flyhigh Hotel API", Version = "v1" });
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
var app = builder.Build();

await DataSeeder.InitializeDatabaseAsync(app);

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI(options => options.SwaggerEndpoint("/swagger/v1/swagger.json", "API v1"));
}
else
{
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseSerilogRequestLogging();

app.UseRouting();

app.UseCors(policy => policy
    .SetIsOriginAllowed(origin => {
        if (string.IsNullOrEmpty(origin)) return false;
        if (origin.Equals("https://h2.mercantec.tech")) return true;
        if (origin.StartsWith("https://localhost") || origin.StartsWith("http://localhost")) return true;
        return false;
    })
    .AllowAnyMethod()
    .AllowAnyHeader()
    .AllowCredentials());
app.UseAuthentication();
app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
    endpoints.MapHub<TicketHub>("/ticketHub");
});

app.Run();