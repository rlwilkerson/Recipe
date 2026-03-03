var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("postgres")
    .AddDatabase("RecipeDb");

builder.AddProject<Projects.Recipe_Web>("recipe-web")
    .WithReference(postgres)
    .WaitFor(postgres);

builder.Build().Run();
