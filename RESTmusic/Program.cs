using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.EntityFrameworkCore;
using RESTmusic.Models;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();
// Skift Repository:Type mellem List og Database i indstillingerne.
string repositoryType = builder.Configuration["Repository:Type"] ?? "List";
if (repositoryType.Equals("Database", StringComparison.OrdinalIgnoreCase))
{
    string connectionString = builder.Configuration.GetConnectionString("MusicDatabase")
        ?? throw new InvalidOperationException("Database kræver en forbindelse med navnet MusicDatabase.");
    if (string.IsNullOrWhiteSpace(connectionString))
        throw new InvalidOperationException("MusicDatabase-forbindelsen må ikke være tom.");

    builder.Services.AddDbContext<CollectionDbContext>(options =>
        options.UseSqlServer(connectionString, sql => sql.EnableRetryOnFailure()));
    builder.Services.AddScoped<ICollectionRepository, CollectionRepositoryDB>();
}
else if (repositoryType.Equals("List", StringComparison.OrdinalIgnoreCase))
{
    builder.Services.AddSingleton<ICollectionRepository, CollectionRepositoryList>();
}
else
{
    throw new InvalidOperationException("Repository:Type skal være List eller Database.");
}

// Samme nøgle, udsteder og modtager bruges til både login og validering.
string? jwtKey = builder.Configuration["Jwt:Key"];
string? jwtIssuer = builder.Configuration["Jwt:Issuer"];
string? jwtAudience = builder.Configuration["Jwt:Audience"];

// Kun lokal udvikling: En ny tilfældig nøgle laves ved hver opstart.
// På Azure skal Jwt__Key sættes som en app-indstilling.
if (builder.Environment.IsDevelopment() && string.IsNullOrWhiteSpace(jwtKey))
{
    jwtKey = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
    builder.Configuration["Jwt:Key"] = jwtKey;
}

if (string.IsNullOrWhiteSpace(jwtKey) || Encoding.UTF8.GetByteCount(jwtKey) < 32)
{
    throw new InvalidOperationException("Sæt Jwt__Key til en hemmelig nøgle på mindst 32 bytes i appens indstillinger.");
}
if (string.IsNullOrWhiteSpace(jwtIssuer) || string.IsNullOrWhiteSpace(jwtAudience))
{
    throw new InvalidOperationException("Jwt:Issuer og Jwt:Audience skal være konfigureret.");
}

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ValidateIssuer = true,
            ValidIssuer = jwtIssuer,
            ValidateAudience = true,
            ValidAudience = jwtAudience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero,
            ValidAlgorithms = new[] { SecurityAlgorithms.HmacSha256 },
            NameClaimType = ClaimTypes.Name,
            RoleClaimType = ClaimTypes.Role
        };
    });
builder.Services.AddAuthorization();

const string AllowLocalCORSpolicy = "AllowLocal";
builder.Services.AddCors(options =>
{
    options.AddPolicy(AllowLocalCORSpolicy, policy =>
    {
        policy.WithOrigins("http://localhost:3000", "http://localhost:5500", "http://127.0.0.1:5500",
                "https://frontmusiccollection-fwa7eybkd0f3hzg6.swedencentral-01.azurewebsites.net")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseCors(AllowLocalCORSpolicy);
// Rækkefølgen er vigtig: Læs tokenet, før brugerens rolle kontrolleres.
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();

// Gør appen tilgængelig for integrationstest med en rigtig HTTP-pipeline.
public partial class Program { }
