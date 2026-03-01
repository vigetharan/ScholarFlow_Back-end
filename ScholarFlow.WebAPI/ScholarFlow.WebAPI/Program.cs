using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using ScholarFlow.Application;
using ScholarFlow.Infrastructure;
using ScholarFlow.Infrastructure.Persistence;
using Scalar.AspNetCore;
using System.Text;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

var builder = WebApplication.CreateBuilder(args);

// ===== 1. Controllers & Basic Services =====
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

// ===== 2. CORS =====
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// ===== 3. Application Layer =====
builder.Services.AddApplication();

// ===== 4. DbContext (FIXED - Correct Retry Syntax) =====
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")!,
        sqlServerOptions => sqlServerOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(30),
            errorNumbersToAdd: null))
    .LogTo(Console.WriteLine, LogLevel.Information));

// ===== 5. Infrastructure Layer =====
builder.Services.AddInfrastructure(builder.Configuration);

// ===== 6. JWT Authentication =====
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Secret"]!))
    };
});

builder.Services.AddAuthorization();

var app = builder.Build();

// ===== 7. DATABASE SEEDING (Single + Error Handling) =====
try
{
    using var scope = app.Services.CreateScope();
    var services = scope.ServiceProvider;
    await DbSeeder.SeedAllAsync(services);
    Console.WriteLine("✅ Database seeded successfully!");
}
catch (Exception ex)
{
    Console.WriteLine($"❌ Seeding failed: {ex.Message}");
    if (app.Environment.IsDevelopment())
        throw;
}

// ===== 8. Development Tools =====
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options =>
    {
        options.WithTitle("ScholarFlow API")
               .WithTheme(ScalarTheme.Purple)
               .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);
    });
}

// ===== 9. Middleware Pipeline =====
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// ===== 10. Global Error Handler (FIXED Syntax) =====
app.Map("/error", (HttpContext context) =>
{
    var exceptionHandlerPathFeature = context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerPathFeature>();
    var exception = exceptionHandlerPathFeature?.Error;

    app.Logger.LogError(exception, "Unhandled exception: {Message}", exception?.Message);

    var problemDetails = new ProblemDetails
    {
        Title = "Internal Server Error",
        Detail = app.Environment.IsDevelopment() ? exception?.StackTrace : null,
        Status = StatusCodes.Status500InternalServerError,
        Type = "https://tools.ietf.org/html/rfc7807",
        Extensions = new Dictionary<string, object?>
        {
            ["traceId"] = System.Diagnostics.Activity.Current?.Id ?? context.TraceIdentifier
        }
    };

    return Results.Problem(problemDetails);
}).ExcludeFromDescription();

app.Run();
