using System.Text;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using TaskManagement.Api.Filters;
using TaskManagement.Api.Middleware;
using TaskManagement.Api.Services;
using TaskManagement.Api.Startup;
using TaskManagement.Application;
using TaskManagement.Application.Common.Interfaces;
using TaskManagement.Infrastructure;
using TaskManagement.Infrastructure.Security;

var builder = WebApplication.CreateBuilder(args);

// --- Application + Infrastructure ---
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// Ownership source: resolves the current user id from the request's JWT claims.
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

// --- Authentication / Authorization ---
var jwtSettings = builder.Configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>()
                  ?? throw new InvalidOperationException("Jwt settings are missing.");

if (string.IsNullOrWhiteSpace(jwtSettings.Key))
    throw new InvalidOperationException(
        "Jwt:Key is not configured. Set it with: dotnet user-secrets set \"Jwt:Key\" \"<secret>\"");

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        // Keep "sub" as "sub" instead of remapping to NameIdentifier.
        options.MapInboundClaims = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidateAudience = true,
            ValidAudience = jwtSettings.Audience,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key)),
            ClockSkew = TimeSpan.FromSeconds(30),
        };
    });

builder.Services.AddAuthorization();

// --- MVC + validation filter ---
builder.Services
    .AddControllers(options =>
    {
        options.Filters.Add<ValidationFilter>();
    })
    .AddJsonOptions(options =>
    {
        // Serialize/accept enums by name ("Pending") instead of number, so the
        // request/response contract matches the documented status values.
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

// --- Global exception handling (ProblemDetails) ---
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

// --- Swagger with a bearer Authorize button ---
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "Task Management API", Version = "v1" });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Paste the JWT access token (without the 'Bearer ' prefix).",
    });

    var bearerReference = new OpenApiSecuritySchemeReference("Bearer");
    options.AddSecurityRequirement(_ => new OpenApiSecurityRequirement
    {
        { bearerReference, new List<string>() },
    });
});

var app = builder.Build();

// In Development, apply migrations and seed a demo user + sample tasks.
if (app.Environment.IsDevelopment())
{
    await app.SeedDatabaseAsync();

    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseExceptionHandler();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
