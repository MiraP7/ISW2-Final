using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using ISW2_Primer_parcial.Data;

namespace ISW2_Primer_parcial.Controllers;

[ApiController]
[Route("api/[controller]")]
public class InventarioController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public InventarioController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: api/Inventario
    [HttpGet]
    public async Task<ActionResult<IEnumerable<object>>> GetInventario()
    {
        var inventario = new List<object>();
        
        try
        {
            using (var connection = _context.Database.GetDbConnection())
            {
                await connection.OpenAsync();
                try
                {
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = "EXEC GetInventario";

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                inventario.Add(new
                                {
                                    IdProducto = reader.GetInt32(0),
                                    Producto = reader.GetString(1),
                                    Existencia = reader.GetInt32(2),
                                    MinimoExistencia = reader.GetInt32(3),
                                    Estado = reader.GetString(4),
                                    UltimaFechaActualizacion = reader.GetDateTime(5)
                                });
                            }
                        }
                    }
                }
                finally
                {
                    if (connection.State == System.Data.ConnectionState.Open)
                        connection.Close();
                }
            }
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { mensaje = "Error interno", detalle = ex.Message });
        }

        return Ok(inventario);
    }

    // GET: api/Inventario/5
    [HttpGet("{idProducto}")]
    public async Task<ActionResult<object>> GetInventarioProducto(int idProducto)
    {
        try
        {
            using (var connection = _context.Database.GetDbConnection())
            {
                await connection.OpenAsync();
                try
                {
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = "EXEC GetInventarioByProducto @IdProducto";
                        command.Parameters.Add(new SqlParameter("@IdProducto", idProducto));

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                var resultado = new
                                {
                                    IdProducto = reader.GetInt32(0),
                                    Producto = reader.GetString(1),
                                    Existencia = reader.GetInt32(2),
                                    MinimoExistencia = reader.GetInt32(3),
                                    Estado = reader.GetString(4),
                                    UltimaFechaActualizacion = reader.GetDateTime(5)
                                };
                                return Ok(resultado);
                            }
                        }
                    }
                }
                finally
                {
                    if (connection.State == System.Data.ConnectionState.Open)
                        connection.Close();
                }
            }
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { mensaje = "Error interno", detalle = ex.Message });
        }

        return NotFound(new { mensaje = "No hay inventario para este producto." });
    }
}
