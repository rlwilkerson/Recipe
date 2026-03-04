using Microsoft.AspNetCore.Identity;
using Recipe.MigrationService;
using Recipe.Web.Data;
using Recipe.Web.Models;
using Recipe.Web.Services;

var builder = Host.CreateApplicationBuilder(args);

builder.AddServiceDefaults();
builder.AddNpgsqlDbContext<AppDbContext>("RecipeDb");

builder.Services.AddIdentityCore<ApplicationUser>()
    .AddEntityFrameworkStores<AppDbContext>();

builder.Services.AddScoped<ISlugService, SlugService>();
builder.Services.AddScoped<IPublicIdService, PublicIdService>();
builder.Services.AddScoped<DatabaseSeeder>();

builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
