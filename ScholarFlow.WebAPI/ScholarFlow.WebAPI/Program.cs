using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using ScholarFlow.Application;
using ScholarFlow.Infrastructure;
using ScholarFlow.Infrastructure.Persistence;
using ScholarFlow.Domain.Interfaces;
using Scalar.AspNetCore;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("http://localhost:4200", "http://your-frontend-domain.com") // Replace with your actual frontend origins
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials(); // Important for cookies/credentials
    });
});
builder.Services.AddEndpointsApiExplorer();

// Use native .NET OpenAPI support (compatible with .NET 10)
builder.Services.AddOpenApi();

// Add Application layer services (MediatR, FluentValidation)
builder.Services.AddApplication();

// Add Infrastructure layer services (DbContext, Repositories, Identity)
builder.Services.AddInfrastructure(builder.Configuration);

// Add JWT Authentication
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

// Seed database (roles and users)
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    await DbSeeder.SeedAllAsync(services);
}

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    // Add Scalar UI for OpenAPI documentation with JWT support
    app.MapScalarApiReference(options =>
    {
        options.WithTitle("ScholarFlow API")
            .WithTheme(ScalarTheme.Purple)
            .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);
    });
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}
else
{
    // Add a global exception handler for production environments
    app.UseExceptionHandler("/error"); 
    // Also consider HSTS for production
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseCors();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Seed academic data (temporarily disabled due to database issues)
// using (var scope = app.Services.CreateScope())
// {
//     var context = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();
//     await ScholarFlow.WebAPI.Data.AcademicSeeder.SeedAcademicDataAsync(context);
// }

// New endpoint for global error handling (optional, but good for structured errors)
app.Map("/error", (HttpContext context) =>
{
    var exceptionHandlerPathFeature = context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerPathFeature>();
    var exception = exceptionHandlerPathFeature?.Error;

    // Log the exception here
    app.Logger.LogError(exception, "An unhandled exception occurred: {Message}", exception?.Message);

    // Return ProblemDetails (RFC 7807)
    return Results.Problem(
        title: "An unexpected error occurred",
        detail: app.Environment.IsDevelopment() ? exception?.StackTrace : null,
        statusCode: StatusCodes.Status500InternalServerError,
        extensions: new Dictionary<string, object?>
        {
            { "traceId", System.Diagnostics.Activity.Current?.Id ?? context.TraceIdentifier },
            { "type", "https://tools.ietf.org/html/rfc7807#section-3.1" } // Standard type
        }
    );
}).ExcludeFromDescription(); // Exclude from OpenAPI docs


app.Run();
