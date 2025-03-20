using GatewaySolution.Configs;
using GatewaySolution.Extensions;
using GatewaySolution.Middlewares;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using Ocelot.Provider.Polly;
using System.Text.Json;
using DotNetEnv;
using System.Text.Json.Nodes;


Env.Load();
var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddEnvironmentVariables();

var jsonConfig = Environment.GetEnvironmentVariable("OCELOT_ROUTES");
var baseUrl = Environment.GetEnvironmentVariable("OCELOT_BASE_URL");



if (string.IsNullOrWhiteSpace(jsonConfig))
{
    throw new Exception("OCELOT_ROUTES is not defined.");
}
try
{
    var routes = JsonNode.Parse(jsonConfig);
    if (routes == null)
    {
        throw new Exception("OCELOT_ROUTES does not contain a valid JSON structure.");
    }
        var fullConfig = new JsonObject
    {
        ["GlobalConfiguration"] = new JsonObject
        {
            ["BaseUrl"] = baseUrl
        },
        ["Routes"] = routes
    };

    var configJson = fullConfig.ToJsonString();
    Console.WriteLine(configJson);

    builder.Configuration.AddJsonStream(new MemoryStream(System.Text.Encoding.UTF8.GetBytes(configJson)));
}
catch (JsonException ex)
{
    throw new Exception("OCELOT_ROUTES contains invalid JSON.", ex);
}


builder.AddAppAuthentication();
builder.Services.AddOcelot(builder.Configuration).AddPolly();
// builder.Services.AddHttpClient<SwaggerLoader>();

var app = builder.Build();

app.MapGet("/", () => "Hello World!");

// var routeLoader = app.Services.GetRequiredService<SwaggerLoader>();
// var routes = await routeLoader.LoadRoutesFromSwagger();

// if (routes.Count > 0)
// {
//     var ocelotConfig = new { Routes = routes };
//     await File.WriteAllTextAsync("ocelot.json", JsonSerializer.Serialize(ocelotConfig));
// }

app.UseMiddleware<OpenApiValidationMiddleware>();
await app.UseOcelot();


app.Run();
