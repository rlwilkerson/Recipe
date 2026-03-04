using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Recipe.MigrationService;
using Recipe.Web.Data;
using Recipe.Web.Models;
using Recipe.Web.Services;

var builder = Host.CreateApplicationBuilder(args);

builder.AddServiceDefaults();
builder.AddNpgsqlDbContext<AppDbContext>("RecipeDb", configureDbContextOptions: options =>
    options.ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning)));

builder.Services.AddIdentityCore<ApplicationUser>()
    .AddEntityFrameworkStores<AppDbContext>();

builder.Services.AddScoped<ISlugService, SlugService>();
builder.Services.AddScoped<IPublicIdService, PublicIdService>();
builder.Services.AddScoped<DatabaseSeeder>();

builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
