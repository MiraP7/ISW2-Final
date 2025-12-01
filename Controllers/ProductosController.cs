using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using ISW2_Primer_parcial.Data;
using ISW2_Primer_parcial.Models;
using ISW2_Primer_parcial.Attributes;
using System.Data;

namespace ISW2_Primer_parcial.Controllers;

[ApiController]
[Route("api/[controller]")]
[RequiereAutenticacion]
public class ProductosController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public ProductosController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: api/Productos
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Producto>>> GetProductos()
    {
        try
        {
            var productos = await _context.Productos.FromSqlInterpolated($"EXEC GetProductos")
                .IgnoreQueryFilters()
                .ToListAsync();
            return productos;
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { mensaje = "Error interno", detalle = ex.Message });
        }
    }

    // GET: api/Productos/5
    [HttpGet("{id}")]
    public async Task<ActionResult<Producto>> GetProducto(int id)
    {
        try
        {
            var producto = await _context.Productos.FromSqlInterpolated($"EXEC GetProducto {id}")
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync();
            
            if (producto == null)
            {
                return NotFound();
            }

            return producto;
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { mensaje = "Error interno", detalle = ex.Message });
        }
    }

    // POST: api/Productos
    [HttpPost]
    public async Task<ActionResult<Producto>> PostProducto(Producto producto)
    {
        int nuevoId = 0;
        string? codigoGenerado = null;
        string? mensaje = null;

        try
        {
            using (var connection = (SqlConnection)_context.Database.GetDbConnection())
            {
                await connection.OpenAsync();
                try
                {
                    using (var command = new SqlCommand("InsertProducto", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        command.Parameters.Add(new SqlParameter("@Nombre", producto.Nombre));
                        command.Parameters.Add(new SqlParameter("@Descripcion", (object?)producto.Descripcion ?? DBNull.Value));
                        command.Parameters.Add(new SqlParameter("@PrecioVenta", producto.PrecioVenta));
                        command.Parameters.Add(new SqlParameter("@MinimoExistencia", producto.MinimoExistencia));
                        
                        var idParam = new SqlParameter("@IdProducto", SqlDbType.Int) { Direction = ParameterDirection.Output };
                        var codigoParam = new SqlParameter("@CodigoProducto", SqlDbType.NVarChar, 50) { Direction = ParameterDirection.Output };
                        var mensajeParam = new SqlParameter("@Mensaje", SqlDbType.NVarChar, 255) { Direction = ParameterDirection.Output };
                        
                        command.Parameters.Add(idParam);
                        command.Parameters.Add(codigoParam);
                        command.Parameters.Add(mensajeParam);

                        await command.ExecuteNonQueryAsync();

                        nuevoId = (int)idParam.Value;
                        codigoGenerado = codigoParam.Value?.ToString();
                        mensaje = mensajeParam.Value?.ToString();
                    }
                }
                finally
                {
                    if (connection.State == ConnectionState.Open)
                        connection.Close();
                }
            }
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { mensaje = "Error interno", detalle = ex.Message });
        }

        if (nuevoId == -1)
        {
            return BadRequest(mensaje);
        }

        producto.IdProducto = nuevoId;
        producto.CodigoProducto = codigoGenerado ?? "";
        return CreatedAtAction(nameof(GetProducto), new { id = nuevoId }, producto);
    }

    // PUT: api/Productos/5
    [HttpPut("{id}")]
    public async Task<IActionResult> PutProducto(int id, Producto producto)
    {
        if (id != producto.IdProducto)
        {
            return BadRequest("El ID de la URL no coincide con el ID del producto.");
        }

        string? mensaje = null;

        try
        {
            using (var connection = (SqlConnection)_context.Database.GetDbConnection())
            {
                await connection.OpenAsync();
                try
                {
                    using (var command = new SqlCommand("UpdateProducto", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        command.Parameters.Add(new SqlParameter("@IdProducto", id));
                        command.Parameters.Add(new SqlParameter("@Nombre", producto.Nombre));
                        command.Parameters.Add(new SqlParameter("@CodigoProducto", (object?)producto.CodigoProducto ?? DBNull.Value));
                        command.Parameters.Add(new SqlParameter("@Descripcion", (object?)producto.Descripcion ?? DBNull.Value));
                        command.Parameters.Add(new SqlParameter("@PrecioVenta", producto.PrecioVenta));
                        command.Parameters.Add(new SqlParameter("@MinimoExistencia", producto.MinimoExistencia));
                        
                        var mensajeParam = new SqlParameter("@Mensaje", SqlDbType.NVarChar, 255) { Direction = ParameterDirection.Output };
                        command.Parameters.Add(mensajeParam);

                        await command.ExecuteNonQueryAsync();

                        mensaje = mensajeParam.Value?.ToString();
                    }
                }
                finally
                {
                    if (connection.State == ConnectionState.Open)
                        connection.Close();
                }
            }
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { mensaje = "Error interno", detalle = ex.Message });
        }

        if (mensaje?.Contains("no encontrado") == true)
        {
            return NotFound(mensaje);
        }

        return NoContent();
    }

    // DELETE: api/Productos/5
    /// <summary>
    /// Eliminar un producto (Solo Administradores)
    /// </summary>
    [HttpDelete("{id}")]
    [RequiereAdmin]
    public async Task<IActionResult> DeleteProducto(int id)
    {
        string? mensaje = null;

        try
        {
            using (var connection = (SqlConnection)_context.Database.GetDbConnection())
            {
                await connection.OpenAsync();
                try
                {
                    using (var command = new SqlCommand("DeleteProducto", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        command.Parameters.Add(new SqlParameter("@IdProducto", id));
                        
                        var mensajeParam = new SqlParameter("@Mensaje", SqlDbType.NVarChar, 255) { Direction = ParameterDirection.Output };
                        command.Parameters.Add(mensajeParam);

                        await command.ExecuteNonQueryAsync();

                        mensaje = mensajeParam.Value?.ToString();
                    }
                }
                finally
                {
                    if (connection.State == ConnectionState.Open)
                        connection.Close();
                }
            }
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { mensaje = "Error interno", detalle = ex.Message });
        }

        if (mensaje?.Contains("no encontrado") == true)
        {
            return NotFound(mensaje);
        }

        // Validar restricciones de integridad
        if (mensaje?.Contains("inventario") == true || mensaje?.Contains("movimientos") == true || mensaje?.Contains("asociados") == true)
        {
            return Conflict(new { error = mensaje, code = "INTEGRITY_CONSTRAINT_VIOLATION" });
        }

        return NoContent();
    }

    // GET: api/Productos/buscar?nombre={termino}
    [HttpGet("buscar")]
    public async Task<ActionResult<IEnumerable<Producto>>> BuscarProductos(string nombre)
    {
        if (string.IsNullOrWhiteSpace(nombre))
        {
            return BadRequest(new { message = "El término de búsqueda es requerido." });
        }

        try
        {
            // Obtenemos todos los productos y filtramos en memoria
            // Nota: Idealmente esto debería ser un SP específico o parámetros en GetProductos
            var productos = await _context.Productos.FromSqlInterpolated($"EXEC GetProductos")
                .IgnoreQueryFilters()
                .ToListAsync();

            var resultados = productos
                .Where(p => p.Nombre.Contains(nombre, StringComparison.OrdinalIgnoreCase))
                .ToList();

            return Ok(resultados);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { mensaje = "Error interno", detalle = ex.Message });
        }
    }
}