using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Recipe.AdminApi.Data;
using Recipe.AdminApi.Features.Users.SearchUsers;
using Recipe.AdminApi.Features.Users.GetUserDetails;
using Recipe.AdminApi.Features.Users.SetUserAccess;
using Recipe.AdminApi.Features.Users.SetAdminRole;

var builder = WebApplication.CreateBuilder(args);

// Aspire service defaults (OpenTelemetry, health checks, service discovery)
builder.AddServiceDefaults();

// EF Core + PostgreSQL
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("RecipeDb")));

// ASP.NET Core Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
    {
        options.SignIn.RequireConfirmedAccount = false;
    })
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

// MediatR — auto-register handlers from this assembly
builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssemblyContaining<Program>());

// Authentication - JWT Bearer for OIDC device flow tokens
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = builder.Configuration["Authentication:Authority"];
        options.Audience = builder.Configuration["Authentication:Audience"];
        options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
        {
            ValidateAudience = true,
            ValidateIssuer = true,
            ValidateLifetime = true
        };
    });

// Authorization - require Admin role for all endpoints
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy =>
        policy.RequireRole("Admin"));
});

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

// User Management Endpoints - all require Admin role
var users = app.MapGroup("/api/users")
    .RequireAuthorization("AdminOnly");

users.MapGet("/search", async (IMediator mediator, string? search, int page = 1, int pageSize = 20) =>
{
    var query = new SearchUsersQuery(search, page, pageSize);
    var result = await mediator.Send(query);
    return Results.Ok(result);
});

users.MapGet("/{userId}", async (IMediator mediator, string userId) =>
{
    try
    {
        var query = new GetUserDetailsQuery(userId);
        var result = await mediator.Send(query);
        return Results.Ok(result);
    }
    catch (InvalidOperationException ex)
    {
        return Results.NotFound(new { error = ex.Message });
    }
});

users.MapPost("/{userId}/access", async (IMediator mediator, string userId, SetUserAccessRequest request) =>
{
    var command = new SetUserAccessCommand(userId, request.EnableAccess);
    var result = await mediator.Send(command);
    return result.Success ? Results.Ok(result) : Results.BadRequest(result);
});

users.MapPost("/{userId}/admin-role", async (IMediator mediator, string userId, SetAdminRoleRequest request) =>
{
    var command = new SetAdminRoleCommand(userId, request.AssignRole);
    var result = await mediator.Send(command);
    return result.Success ? Results.Ok(result) : Results.BadRequest(result);
});

app.MapDefaultEndpoints();

app.Run();

// Request DTOs
public record SetUserAccessRequest(bool EnableAccess);
public record SetAdminRoleRequest(bool AssignRole);

// Make Program accessible for integration tests
public partial class Program { }

