using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using System.Text;

namespace GatewaySolution.Middlewares
{
    public class OpenApiValidationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<OpenApiValidationMiddleware> _logger;
        private readonly Dictionary<string, JSchema> _schemas;

        public OpenApiValidationMiddleware(RequestDelegate next, ILogger<OpenApiValidationMiddleware> logger)
        {
            _next = next;
            _logger = logger;
            // _schemas = LoadSchemas(); // Cargar todos los esquemas en memoria
        }

        private Dictionary<string, JSchema> LoadSchemas()
        {
            var schemas = new Dictionary<string, JSchema>();

            foreach (var file in Directory.GetFiles("Schemas", "*.json"))
            {
                var schemaName = Path.GetFileNameWithoutExtension(file);
                var schemaContent = File.ReadAllText(file);
                schemas[schemaName] = JSchema.Parse(schemaContent);
            }

            return schemas;
        }

        public async Task Invoke(HttpContext context)
        {
            if (context.Request.Method == HttpMethods.Post || context.Request.Method == HttpMethods.Put)
            {
                string path = context.Request.Path.Value.ToLower();

                // Determinar qué esquema usar según la ruta
                string schemaKey = GetSchemaKey(path);
                if (schemaKey != null && _schemas.TryGetValue(schemaKey, out JSchema schema))
                {
                    context.Request.EnableBuffering();
                    using var reader = new StreamReader(context.Request.Body, Encoding.UTF8, leaveOpen: true);
                    var body = await reader.ReadToEndAsync();
                    context.Request.Body.Position = 0;

                    var json = JToken.Parse(body);
                    if (!json.IsValid(schema, out IList<ValidationError> errors))
                    {
                        _logger.LogWarning("Payload inválido para {schemaKey}: {errors}", schemaKey, errors);
                        context.Response.StatusCode = StatusCodes.Status400BadRequest;
                        await context.Response.WriteAsync($"Solicitud inválida según OpenAPI ({schemaKey}).");
                        return;
                    }
                }
            }

            await _next(context);
        }

        private string GetSchemaKey(string path)
        {
            if (path.StartsWith("/api/serviceA")) return "serviceA-schema";
            if (path.StartsWith("/api/serviceB")) return "serviceB-schema";
            if (path.StartsWith("/api/serviceC")) return "serviceC-schema";
            return null; // No validar si no hay esquema para esta ruta
        }
    }
}
