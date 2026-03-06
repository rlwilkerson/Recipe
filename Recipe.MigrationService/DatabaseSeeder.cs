using Bogus;
using Microsoft.AspNetCore.Identity;
using Recipe.Web.Data;
using Recipe.Web.Models;
using Recipe.Web.Services;

namespace Recipe.MigrationService;

public class DatabaseSeeder(
    AppDbContext db,
    UserManager<ApplicationUser> userManager,
    RoleManager<IdentityRole> roleManager,
    ISlugService slugService,
    IPublicIdService publicIdService,
    ILogger<DatabaseSeeder> logger)
{
    private const int BogusFixedSeed = 42;

    // Seed users: email, password, display name, isAdmin
    private static readonly (string Email, string Password, string DisplayName, bool IsAdmin)[] SeedUsers =
    [
        ("admin@recipe.test",      "Admin@12345!", "Admin User", true),
        ("testuser@cookbook.test", "Test@12345!", "Test User", false),
        ("alice@recipe.test",      "Seed@12345!", "Alice Baker", false),
        ("bob@recipe.test",        "Seed@12345!", "Bob Cook", false),
    ];

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        // Idempotent: skip if first seed user already exists
        var firstEmail = SeedUsers[0].Email;
        if (await userManager.FindByEmailAsync(firstEmail) is not null)
        {
            logger.LogInformation("Seed data already present, skipping.");
            return;
        }

        logger.LogInformation("Seeding database...");

        // Ensure Admin role exists
        if (!await roleManager.RoleExistsAsync("Admin"))
        {
            await roleManager.CreateAsync(new IdentityRole("Admin"));
            logger.LogInformation("Created Admin role");
        }

        var faker = new Faker { Random = new Randomizer(BogusFixedSeed) };

        var createdUsers = new List<ApplicationUser>();
        foreach (var (email, password, displayName, isAdmin) in SeedUsers)
        {
            var user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true,
                DisplayName = displayName,
            };
            var result = await userManager.CreateAsync(user, password);
            if (!result.Succeeded)
            {
                logger.LogWarning("Failed to create user {Email}: {Errors}", email,
                    string.Join(", ", result.Errors.Select(e => e.Description)));
                continue;
            }

            // Assign admin role if needed
            if (isAdmin)
            {
                await userManager.AddToRoleAsync(user, "Admin");
                logger.LogInformation("Created admin user {Email}", email);
            }
            else
            {
                logger.LogInformation("Created user {Email}", email);
            }

            createdUsers.Add(user);
        }

        if (createdUsers.Count == 0)
            return;

        // Cookbook and recipe names for deterministic generation
        var cookbookTitles = new[]
        {
            "Quick Weeknight Dinners", "Baking Basics", "Healthy Meal Prep",
            "Sunday Slow Cooker", "Vegan Essentials", "Grilling Favorites",
        };

        var recipeTemplates = new[]
        {
            ("Garlic Butter Pasta", "A simple, flavorful pasta dish ready in 20 minutes.", "200g spaghetti\n4 cloves garlic\n3 tbsp butter\nSalt and pepper", "Cook pasta. Sauté garlic in butter. Toss and serve."),
            ("Overnight Oats", "Creamy oats prepared the night before for a quick breakfast.", "1 cup rolled oats\n1 cup milk\n2 tbsp honey\nFresh berries", "Mix oats, milk, and honey. Refrigerate overnight. Top with berries."),
            ("Classic Chili", "Hearty beef chili with beans and bold spices.", "500g ground beef\n1 can kidney beans\n1 can diced tomatoes\nChili powder, cumin", "Brown beef. Add remaining ingredients. Simmer 30 minutes."),
            ("Avocado Toast", "Quick and nutritious breakfast or snack.", "2 slices sourdough\n1 avocado\nLemon juice\nRed pepper flakes", "Toast bread. Mash avocado with lemon. Spread on toast and season."),
            ("Chicken Stir Fry", "Fast and colorful stir fry with crisp vegetables.", "300g chicken breast\n2 cups mixed vegetables\n3 tbsp soy sauce\n1 tbsp sesame oil", "Slice chicken. Stir fry with vegetables. Add sauces and toss."),
            ("Banana Pancakes", "Fluffy pancakes made with ripe bananas.", "2 bananas\n2 eggs\n1 cup flour\n1 tsp baking powder\n1 cup milk", "Mash bananas. Mix all ingredients. Cook on medium heat until golden."),
            ("Greek Salad", "Fresh Mediterranean salad with feta and olives.", "3 tomatoes\n1 cucumber\n1/2 red onion\n100g feta\n1/2 cup olives\nOlive oil", "Chop vegetables. Combine with feta and olives. Drizzle olive oil."),
            ("Lemon Roast Chicken", "Juicy roasted chicken with fresh herbs.", "1 whole chicken\n2 lemons\nFresh rosemary\nOlive oil\nGarlic", "Rub chicken with lemon, garlic, and rosemary. Roast at 200°C for 1.5 hours."),
            ("Tomato Soup", "Classic creamy tomato soup from scratch.", "6 tomatoes\n1 onion\n2 cups vegetable broth\n1/2 cup cream", "Sauté onion. Add tomatoes and broth. Simmer 20 min. Blend, add cream."),
            ("Blueberry Muffins", "Moist muffins packed with fresh blueberries.", "2 cups flour\n1 cup sugar\n1 cup blueberries\n2 eggs\n1/2 cup butter\n1 cup milk", "Mix dry ingredients. Beat wet. Combine, fold in berries. Bake 20 min at 190°C."),
            ("Beef Tacos", "Simple seasoned beef tacos with fresh toppings.", "500g ground beef\nTaco seasoning\n8 small tortillas\nLettuce, tomato, cheese", "Brown beef with seasoning. Warm tortillas. Assemble with toppings."),
            ("Vegetable Curry", "Aromatic curry loaded with seasonal vegetables.", "2 cups mixed vegetables\n1 can coconut milk\n2 tbsp curry paste\n1 onion\nFresh cilantro", "Sauté onion. Add curry paste. Add vegetables and coconut milk. Simmer 20 min."),
        };

        var recipeIndex = 0;
        var cookbookIndex = 0;
        var allCookbooks = new List<(ApplicationUser Owner, Cookbook Cookbook)>();

        // Only seed cookbooks for non-admin users
        var regularUsers = createdUsers.Where(u => !SeedUsers.Any(s => s.Email == u.Email && s.IsAdmin)).ToList();

        foreach (var user in regularUsers)
        {
            for (int c = 0; c < 2; c++)
            {
                var cookbookTitle = cookbookTitles[cookbookIndex++ % cookbookTitles.Length];
                var publicId = await publicIdService.GenerateForCookbookAsync(db);
                var slug = slugService.GenerateSlug(cookbookTitle);

                var cookbook = new Cookbook
                {
                    Name = cookbookTitle,
                    Description = $"A collection of {cookbookTitle.ToLower()} recipes.",
                    OwnerId = user.Id,
                    PublicId = publicId,
                    Slug = slug,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                };
                db.Cookbooks.Add(cookbook);
                await db.SaveChangesAsync(cancellationToken);

                allCookbooks.Add((user, cookbook));
                logger.LogInformation("Created cookbook '{Name}' for {Email}", cookbookTitle, user.Email);

                // 3–5 recipes per cookbook (deterministic: 3 for first, 5 for second, etc.)
                int recipeCount = (c == 0) ? 3 : 5;
                for (int r = 0; r < recipeCount; r++)
                {
                    var (title, description, ingredients, instructions) = recipeTemplates[recipeIndex++ % recipeTemplates.Length];

                    var rPublicId = await publicIdService.GenerateForRecipeAsync(db);
                    var rSlug = slugService.GenerateSlug(title);

                    var recipe = new Recipe.Web.Models.Recipe
                    {
                        Title = title,
                        Description = description,
                        Ingredients = ingredients,
                        Instructions = instructions,
                        PrepTime = faker.Random.Int(5, 30),
                        CookTime = faker.Random.Int(10, 60),
                        Servings = faker.Random.Int(2, 6),
                        OwnerId = user.Id,
                        PublicId = rPublicId,
                        Slug = rSlug,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow,
                    };
                    db.Recipes.Add(recipe);
                    await db.SaveChangesAsync(cancellationToken);

                    db.CookbookRecipes.Add(new CookbookRecipe
                    {
                        CookbookId = cookbook.Id,
                        RecipeId = recipe.Id,
                    });
                    await db.SaveChangesAsync(cancellationToken);
                }
            }
        }

        // Seed one share: alice's first cookbook → bob (Read permission)
        var alice = createdUsers.FirstOrDefault(u => u.Email == "alice@recipe.test");
        var bob = createdUsers.FirstOrDefault(u => u.Email == "bob@recipe.test");
        var aliceFirstCookbook = allCookbooks.FirstOrDefault(x => x.Owner.Id == alice?.Id).Cookbook;

        if (alice is not null && bob is not null && aliceFirstCookbook is not null)
        {
            db.Shares.Add(new Share
            {
                Scope = ShareScope.Cookbook,
                Permission = SharePermission.Read,
                OwnerId = alice.Id,
                TargetUserId = bob.Id,
                CookbookId = aliceFirstCookbook.Id,
                CreatedAt = DateTime.UtcNow,
            });
            await db.SaveChangesAsync(cancellationToken);
            logger.LogInformation("Seeded share: alice's cookbook '{Name}' → bob", aliceFirstCookbook.Name);
        }

        logger.LogInformation("Seeding complete.");
    }
}
