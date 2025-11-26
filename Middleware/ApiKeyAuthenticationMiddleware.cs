using ISW2_Primer_parcial.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace ISW2_Primer_parcial.Middleware;

public class ApiKeyAuthenticationMiddleware
{
    private readonly RequestDelegate _next;
    private const string ApiKeyHeader = "X-API-Key";

    public ApiKeyAuthenticationMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, ApplicationDbContext dbContext)
    {
        try
        {
            var path = context.Request.Path.Value?.ToLower() ?? "";

            // Rutas que no requieren autenticación (Swagger, documentación, etc)
            if (path.Contains("/swagger") || 
                path.Contains("/openapi") ||
                path.Contains(".js") ||
                path.Contains(".css") ||
                path.Contains(".png"))
            {
                await _next(context);
                return;
            }

            // Obtener API Key del header
            if (!context.Request.Headers.TryGetValue(ApiKeyHeader, out var apiKeyValue))
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsJsonAsync(new { mensaje = "API Key no proporcionada" });
                return;
            }

            var apiKey = apiKeyValue.ToString();

            // Validar la API Key
            var keyValida = await dbContext.ApiKeys.FirstOrDefaultAsync(k =>
                k.Clave == apiKey &&
                k.Activa &&
                (k.FechaVencimiento == null || k.FechaVencimiento > DateTime.UtcNow));

            if (keyValida == null)
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsJsonAsync(new { mensaje = "API Key inválida o expirada" });
                return;
            }

            // Actualizar fecha de último uso (no esperar, es una actualización secundaria)
            try
            {
                keyValida.UltimaFechaUso = DateTime.UtcNow;
                await dbContext.SaveChangesAsync();
            }
            catch
            {
                // Ignorar errores en actualización de última fecha de uso
            }

            // Pasar a la siguiente middleware
            await _next(context);
        }
        catch (Exception ex)
        {
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsJsonAsync(new { mensaje = "Error interno", detalle = ex.Message });
        }
    }
}
