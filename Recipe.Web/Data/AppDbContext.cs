using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Recipe.Web.Models;

namespace Recipe.Web.Data;

public class AppDbContext : IdentityDbContext<ApplicationUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Cookbook> Cookbooks => Set<Cookbook>();
    public DbSet<Models.Recipe> Recipes => Set<Models.Recipe>();
    public DbSet<CookbookRecipe> CookbookRecipes => Set<CookbookRecipe>();
    public DbSet<Share> Shares => Set<Share>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Cookbook
        modelBuilder.Entity<Cookbook>(e =>
        {
            e.HasKey(c => c.Id);
            e.HasIndex(c => c.PublicId).IsUnique();
            e.Property(c => c.Name).IsRequired().HasMaxLength(200);
            e.Property(c => c.PublicId).IsRequired().HasMaxLength(20);
            e.Property(c => c.Slug).IsRequired().HasMaxLength(200);
            e.HasOne(c => c.Owner).WithMany().HasForeignKey(c => c.OwnerId).OnDelete(DeleteBehavior.Cascade);
        });

        // Recipe
        modelBuilder.Entity<Models.Recipe>(e =>
        {
            e.HasKey(r => r.Id);
            e.HasIndex(r => r.PublicId).IsUnique();
            e.Property(r => r.Title).IsRequired().HasMaxLength(300);
            e.Property(r => r.PublicId).IsRequired().HasMaxLength(20);
            e.Property(r => r.Slug).IsRequired().HasMaxLength(300);
            e.HasOne(r => r.Owner).WithMany().HasForeignKey(r => r.OwnerId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(r => r.OriginalRecipe).WithMany().HasForeignKey(r => r.OriginalRecipeId).OnDelete(DeleteBehavior.Restrict);
        });

        // CookbookRecipe join table
        modelBuilder.Entity<CookbookRecipe>(e =>
        {
            e.HasKey(cr => new { cr.CookbookId, cr.RecipeId });
            e.HasOne(cr => cr.Cookbook).WithMany(c => c.CookbookRecipes).HasForeignKey(cr => cr.CookbookId);
            e.HasOne(cr => cr.Recipe).WithMany(r => r.CookbookRecipes).HasForeignKey(cr => cr.RecipeId);
        });

        // Share
        modelBuilder.Entity<Share>(e =>
        {
            e.HasKey(s => s.Id);
            e.HasOne(s => s.Owner).WithMany().HasForeignKey(s => s.OwnerId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(s => s.TargetUser).WithMany().HasForeignKey(s => s.TargetUserId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(s => s.Cookbook).WithMany(c => c.Shares).HasForeignKey(s => s.CookbookId).IsRequired(false);
            e.HasOne(s => s.Recipe).WithMany(r => r.Shares).HasForeignKey(s => s.RecipeId).IsRequired(false);
        });
    }
}
