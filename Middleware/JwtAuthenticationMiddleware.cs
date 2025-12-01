using ISW2_Primer_parcial.Data;
using ISW2_Primer_parcial.Services;
using ISW2_Primer_parcial.Attributes;
using Microsoft.EntityFrameworkCore;

namespace ISW2_Primer_parcial.Middleware;

public class JwtAuthenticationMiddleware
{
    private readonly RequestDelegate _next;

    public JwtAuthenticationMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, ApplicationDbContext dbContext, IJwtService jwtService)
    {
        try
        {
            var path = context.Request.Path.Value?.ToLower() ?? "";

            // Rutas que NUNCA requieren autenticación
            if (path.Contains("/swagger") || 
                path.Contains("/openapi") ||
                path.Contains(".js") ||
                path.Contains(".css") ||
                path.Contains(".png") ||
                path.Contains("/api/auth/login") ||
                path.Contains("/api/auth/registro"))
            {
                await _next(context);
                return;
            }

            // Verificar si el endpoint tiene atributo [PermitirAnonimo]
            var endpoint = context.GetEndpoint();
            if (endpoint?.Metadata?.GetMetadata<PermitirAnonimoAttribute>() != null)
            {
                await _next(context);
                return;
            }

            // Obtener token del header Authorization
            var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();
            
            if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsJsonAsync(new { mensaje = "Token de autenticación no proporcionado" });
                return;
            }

            var token = authHeader.Substring("Bearer ".Length).Trim();
            var userId = jwtService.ValidarToken(token);

            if (userId == null)
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsJsonAsync(new { mensaje = "Token inválido o expirado" });
                return;
            }

            // Obtener usuario con su rol
            var usuario = await dbContext.Usuarios
                .Include(u => u.Rol)
                .FirstOrDefaultAsync(u => u.IdUsuario == userId && u.Activo);

            if (usuario == null)
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsJsonAsync(new { mensaje = "Usuario no encontrado o inactivo" });
                return;
            }

            // Guardar información del usuario en el contexto para usar en los controladores
            context.Items["Usuario"] = usuario;
            context.Items["IdUsuario"] = usuario.IdUsuario;
            context.Items["Rol"] = usuario.Rol?.Nombre ?? "Usuario";
            context.Items["IdRol"] = usuario.IdRol;

            // Actualizar último acceso
            try
            {
                usuario.UltimoAcceso = DateTime.UtcNow;
                await dbContext.SaveChangesAsync();
            }
            catch
            {
                // Ignorar errores de actualización de último acceso
            }

            await _next(context);
        }
        catch (Exception ex)
        {
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsJsonAsync(new { mensaje = "Error interno en autenticación", detalle = ex.Message });
        }
    }
}
