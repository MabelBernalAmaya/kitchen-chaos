using Microsoft.Azure.Cosmos;
using KitchenChaos.Server.Hubs;
using KitchenChaos.Server.Repositories;
using KitchenChaos.Server.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSignalR();
builder.Services.AddSingleton<RoomService>();

// Cosmos DB: cliente singleton reutilizable
builder.Services.AddSingleton(_ =>
{
    var cfg = builder.Configuration;
    return new CosmosClient(
        cfg["CosmosDb:AccountEndpoint"]!,
        cfg["CosmosDb:AccountKey"]!,
        new CosmosClientOptions { SerializerOptions = new() { PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase } }
    );
});

builder.Services.AddScoped<IPlayerProfileRepository, CosmosPlayerProfileRepository>();
builder.Services.AddScoped<PlayerProfileService>();

var app = builder.Build();

// Crear base de datos y contenedor si no existen (útil en local con el emulador)
using (var scope = app.Services.CreateScope())
{
    var cosmos = scope.ServiceProvider.GetRequiredService<CosmosClient>();
    var cfg = app.Configuration;
    var db = await cosmos.CreateDatabaseIfNotExistsAsync(cfg["CosmosDb:DatabaseName"]!);
    await db.Database.CreateContainerIfNotExistsAsync(cfg["CosmosDb:ContainerName"]!, "/id");
}

app.UseDefaultFiles();
app.UseStaticFiles();

app.MapHub<GameHub>("/gamehub");

app.Run();
