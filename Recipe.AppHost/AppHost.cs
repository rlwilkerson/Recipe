var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("postgres")
    .AddDatabase("RecipeDb");

var migrations = builder.AddProject<Projects.Recipe_MigrationService>("migrations")
    .WithReference(postgres)
    .WaitFor(postgres);

builder.AddProject<Projects.Recipe_Web>("recipe-web")
    .WithReference(postgres)
    .WaitForCompletion(migrations);

builder.AddProject<Projects.Recipe_AdminApi>("recipe-adminapi")
    .WithReference(postgres)
    .WaitForCompletion(migrations);

builder.AddDockerComposeEnvironment("compose");

builder.Build().Run();
