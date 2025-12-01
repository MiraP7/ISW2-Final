using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ISW2_Primer_parcial.Attributes;

/// <summary>
/// Atributo para requerir autenticación JWT
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class RequiereAutenticacionAttribute : Attribute, IAuthorizationFilter
{
    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var usuario = context.HttpContext.Items["Usuario"];
        if (usuario == null)
        {
            context.Result = new JsonResult(new { mensaje = "No autorizado. Se requiere autenticación." })
            {
                StatusCode = StatusCodes.Status401Unauthorized
            };
        }
    }
}

/// <summary>
/// Atributo para requerir rol de Administrador
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class RequiereAdminAttribute : Attribute, IAuthorizationFilter
{
    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var usuario = context.HttpContext.Items["Usuario"];
        var rol = context.HttpContext.Items["Rol"]?.ToString();

        if (usuario == null)
        {
            context.Result = new JsonResult(new { mensaje = "No autorizado. Se requiere autenticación." })
            {
                StatusCode = StatusCodes.Status401Unauthorized
            };
            return;
        }

        if (rol != "Administrador")
        {
            context.Result = new JsonResult(new { mensaje = "Acceso denegado. Se requiere rol de Administrador." })
            {
                StatusCode = StatusCodes.Status403Forbidden
            };
        }
    }
}

/// <summary>
/// Atributo para permitir acceso sin autenticación
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class PermitirAnonimoAttribute : Attribute
{
}
