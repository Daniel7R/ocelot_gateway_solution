using GatewaySolution.Configs;
using GatewaySolution.Extensions;
using GatewaySolution.Middlewares;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

builder.AddAppAuthentication();
builder.Services.AddOcelot();
builder.Services.AddHttpClient<SwaggerLoader>();

var app = builder.Build();

app.MapGet("/", () => "Hello World!");

var routeLoader = app.Services.GetRequiredService<SwaggerLoader>();
var routes = await routeLoader.LoadRoutesFromSwagger();

if (routes.Count > 0)
{
    var ocelotConfig = new { Routes = routes };
    await File.WriteAllTextAsync("ocelot.json", JsonSerializer.Serialize(ocelotConfig));
}

app.UseMiddleware<OpenApiValidationMiddleware>();
await app.UseOcelot();


app.Run();
