using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using GatewaySolution.models;

namespace GatewaySolution.Configs
{
    public class SwaggerLoader
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public SwaggerLoader(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public async Task<List<OcelotRoute>> LoadRoutesFromSwagger()
        {
            var services = _configuration.GetSection("SwaggerServices").Get<List<string>>();
            var routes = new List<OcelotRoute>();

            foreach (var service in services)
            {
                try
                {
                    var swaggerJson = await _httpClient.GetStringAsync($"{service}/swagger/v1/swagger.json");
                    var swaggerDoc = JsonConvert.DeserializeObject<JObject>(swaggerJson);
                    var paths = swaggerDoc["paths"] as JObject;

                    if (paths != null)
                    {
                        foreach (var path in paths)
                        {
                            foreach (var method in path.Value.Children<JProperty>())
                            {
                                routes.Add(new OcelotRoute
                                {
                                    DownstreamPathTemplate = path.Key,
                                    DownstreamScheme = "https",
                                    DownstreamHostAndPorts = new List<HostAndPort> { new() { Host = new Uri(service).Host, Port = new Uri(service).Port } },
                                    UpstreamPathTemplate = path.Key,
                                    UpstreamHttpMethod = new List<string> { method.Name.ToUpper() }
                                });
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error cargando Swagger desde {service}: {ex.Message}");
                }
            }

            return routes;
        }
    }
}
