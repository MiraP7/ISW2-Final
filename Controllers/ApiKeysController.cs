using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ISW2_Primer_parcial.Data;
using ISW2_Primer_parcial.Models;
using System.Security.Cryptography;
using System.Text;

namespace ISW2_Primer_parcial.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ApiKeysController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public ApiKeysController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: api/ApiKeys
    [HttpGet]
    public async Task<ActionResult<IEnumerable<object>>> GetApiKeys()
    {
        var keys = await _context.ApiKeys
            .Select(k => new
            {
                k.IdApiKey,
                k.Nombre,
                k.Descripcion,
                k.Activa,
                ClaveUltimosCaracteres = k.Clave.Substring(k.Clave.Length - 4),
                k.FechaCreacion,
                k.FechaVencimiento,
                k.UltimaFechaUso
            })
            .ToListAsync();

        return Ok(keys);
    }

    // POST: api/ApiKeys
    [HttpPost]
    public async Task<ActionResult<object>> CreateApiKey(CreateApiKeyRequest request)
    {
        // Generar clave única
        var clave = GenerarApiKey();

        var apiKey = new ApiKey
        {
            Clave = clave,
            Nombre = request.Nombre,
            Descripcion = request.Descripcion,
            Activa = true,
            FechaVencimiento = request.DiasVencimiento.HasValue
                ? DateTime.UtcNow.AddDays(request.DiasVencimiento.Value)
                : null
        };

        _context.ApiKeys.Add(apiKey);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetApiKeys), new
        {
            idApiKey = apiKey.IdApiKey,
            nombre = apiKey.Nombre,
            clave = clave,
            descripcion = apiKey.Descripcion,
            activa = apiKey.Activa,
            fechaVencimiento = apiKey.FechaVencimiento,
            mensaje = "⚠️ Guarda la clave en un lugar seguro. No podrás verla nuevamente."
        });
    }

    // PUT: api/ApiKeys/{id}/desactivar
    [HttpPut("{id}/desactivar")]
    public async Task<IActionResult> DeactivateApiKey(int id)
    {
        var apiKey = await _context.ApiKeys.FindAsync(id);
        if (apiKey == null)
        {
            return NotFound();
        }

        apiKey.Activa = false;
        await _context.SaveChangesAsync();

        return NoContent();
    }

    // PUT: api/ApiKeys/{id}/activar
    [HttpPut("{id}/activar")]
    public async Task<IActionResult> ActivateApiKey(int id)
    {
        var apiKey = await _context.ApiKeys.FindAsync(id);
        if (apiKey == null)
        {
            return NotFound();
        }

        apiKey.Activa = true;
        await _context.SaveChangesAsync();

        return NoContent();
    }

    // DELETE: api/ApiKeys/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteApiKey(int id)
    {
        var apiKey = await _context.ApiKeys.FindAsync(id);
        if (apiKey == null)
        {
            return NotFound();
        }

        _context.ApiKeys.Remove(apiKey);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private static string GenerarApiKey()
    {
        const string caracteres = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        var buffer = new byte[32];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(buffer);
        }

        var resultado = new StringBuilder();
        foreach (byte b in buffer)
        {
            resultado.Append(caracteres[b % caracteres.Length]);
        }

        return "sk_" + resultado.ToString();
    }
}

public class CreateApiKeyRequest
{
    public required string Nombre { get; set; }
    public string? Descripcion { get; set; }
    public int? DiasVencimiento { get; set; }
}
