using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ISW2_Primer_parcial.Data;
using ISW2_Primer_parcial.Models;

namespace ISW2_Primer_parcial.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MovimientosInventarioController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public MovimientosInventarioController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: api/MovimientosInventario
    [HttpGet]
    public async Task<ActionResult<IEnumerable<MovimientosInventario>>> GetMovimientos()
    {
        return await _context.MovimientosInventario
            .Include(m => m.Producto)
            .Include(m => m.TipoMovimiento)
            .ToListAsync();
    }

    // GET: api/MovimientosInventario/5
    [HttpGet("{id}")]
    public async Task<ActionResult<MovimientosInventario>> GetMovimiento(int id)
    {
        var movimiento = await _context.MovimientosInventario
            .Include(m => m.Producto)
            .Include(m => m.TipoMovimiento)
            .FirstOrDefaultAsync(m => m.IdMovimiento == id);

        if (movimiento == null)
        {
            return NotFound();
        }

        return movimiento;
    }

    // GET: api/MovimientosInventario/por-producto/5
    [HttpGet("por-producto/{idProducto}")]
    public async Task<ActionResult<IEnumerable<MovimientosInventario>>> GetMovimientosPorProducto(int idProducto)
    {
        var movimientos = await _context.MovimientosInventario
            .Where(m => m.IdProductoAsociado == idProducto)
            .Include(m => m.Producto)
            .Include(m => m.TipoMovimiento)
            .ToListAsync();

        return movimientos;
    }

    // POST: api/MovimientosInventario
    [HttpPost]
    public async Task<ActionResult<object>> PostMovimiento(MovimientosInventario movimiento)
    {
        // Validar que el producto existe
        var producto = await _context.Productos.FirstOrDefaultAsync(p => p.IdProducto == movimiento.IdProductoAsociado);
        if (producto == null)
        {
            return NotFound(new { mensaje = "Producto no encontrado." });
        }

        // Obtener o crear registro en Inventario
        var inventario = await _context.Inventarios.FirstOrDefaultAsync(i => i.IdProducto == movimiento.IdProductoAsociado);
        if (inventario == null)
        {
            inventario = new Inventario { IdProducto = movimiento.IdProductoAsociado, Existencia = 0 };
            _context.Inventarios.Add(inventario);
        }

        // Obtener el tipo de movimiento
        var tipoMovimiento = await _context.TipoMovimientos.FirstOrDefaultAsync(t => t.IdTipoMovimiento == movimiento.IdTipoMovimiento);
        if (tipoMovimiento == null)
        {
            return BadRequest(new { mensaje = "Tipo de movimiento no válido." });
        }

        // Aplicar lógica según el tipo de movimiento (R4)
        if (tipoMovimiento.Tipo == "Entrada")
        {
            // Entrada: sumar cantidad a existencia
            inventario.Existencia += movimiento.Cantidad;
        }
        else if (tipoMovimiento.Tipo == "Salida")
        {
            // Salida: restar cantidad de existencia (R5)
            // Validar que no quede negativo
            if (inventario.Existencia - movimiento.Cantidad < 0)
            {
                return BadRequest(new { mensaje = "No hay suficiente inventario para realizar esta transacción de salida." });
            }

            // Si MinimoExistencia es 0, no permitir que quede negativo (ya validado arriba)
            if (producto.MinimoExistencia == 0 && inventario.Existencia - movimiento.Cantidad < 0)
            {
                return BadRequest(new { mensaje = "El inventario no puede quedar negativo." });
            }

            // Restar cantidad
            inventario.Existencia -= movimiento.Cantidad;

            // Si llega al MinimoExistencia, notificar
            // Esta notificación se incluirá en la respuesta exitosa
        }
        else
        {
            return BadRequest(new { mensaje = "Tipo de movimiento no reconocido." });
        }

        // Actualizar fecha de última actualización
        inventario.UltimaFechaActualizacion = DateTime.Now;
        movimiento.Fecha = DateTime.Now;
        movimiento.UltimaFechaActualizacion = DateTime.Now;

        // Guardar movimiento
        _context.MovimientosInventario.Add(movimiento);
        await _context.SaveChangesAsync();

        // Preparar respuesta con notificación si aplica (R5)
        var notificacion = (string?)null;

        // Si es salida y llegó al MinimoExistencia, añadir notificación
        if (tipoMovimiento.Tipo == "Salida" && inventario.Existencia <= producto.MinimoExistencia && producto.MinimoExistencia > 0)
        {
            notificacion = $"⚠️ ADVERTENCIA: Inventario por debajo del mínimo. Existencia actual: {inventario.Existencia}, Mínimo requerido: {producto.MinimoExistencia}";
        }

        var respuesta = new
        {
            movimiento = movimiento,
            inventarioActual = inventario.Existencia,
            notificacion = notificacion
        };

        return CreatedAtAction(nameof(GetMovimiento), new { id = movimiento.IdMovimiento }, respuesta);
    }

    // PUT: api/MovimientosInventario/5
    [HttpPut("{id}")]
    public async Task<IActionResult> PutMovimiento(int id, MovimientosInventario movimiento)
    {
        if (id != movimiento.IdMovimiento)
        {
            return BadRequest();
        }

        var movimientoExistente = await _context.MovimientosInventario.FindAsync(id);
        if (movimientoExistente == null)
        {
            return NotFound();
        }

        movimientoExistente.Fecha = movimiento.Fecha;
        movimientoExistente.Cantidad = movimiento.Cantidad;
        movimientoExistente.IdTipoMovimiento = movimiento.IdTipoMovimiento;
        movimientoExistente.UltimaFechaActualizacion = DateTime.Now;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!MovimientoExists(id))
            {
                return NotFound();
            }
            throw;
        }

        return NoContent();
    }

    // DELETE: api/MovimientosInventario/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteMovimiento(int id)
    {
        var movimiento = await _context.MovimientosInventario.FindAsync(id);
        if (movimiento == null)
        {
            return NotFound();
        }

        _context.MovimientosInventario.Remove(movimiento);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool MovimientoExists(int id)
    {
        return _context.MovimientosInventario.Any(e => e.IdMovimiento == id);
    }
}
