using ISW2_Primer_parcial.Models;
using Microsoft.EntityFrameworkCore;

namespace ISW2_Primer_parcial.Data;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(ApplicationDbContext context)
    {
        var now = DateTime.UtcNow;
        var blueprint = new List<(Producto Producto, int Stock)>
        {
            (new Producto
            {
                Nombre = "Laptop Pro 14",
                CodigoProducto = "PRD-LP14",
                Descripcion = "Equipo portátil para trabajo pesado",
                PrecioVenta = 1299.99m,
                MinimoExistencia = 5,
                Eliminado = false,
                FechaCreacion = now,
                UltimaFechaActualizacion = now
            }, 12),
            (new Producto
            {
                Nombre = "Monitor UltraWide 34",
                CodigoProducto = "PRD-MW34",
                Descripcion = "Monitor IPS 144Hz",
                PrecioVenta = 799.50m,
                MinimoExistencia = 3,
                Eliminado = false,
                FechaCreacion = now,
                UltimaFechaActualizacion = now
            }, 6),
            (new Producto
            {
                Nombre = "Mouse Ergo MX",
                CodigoProducto = "PRD-MERGO",
                Descripcion = "Mouse inalámbrico ergonómico",
                PrecioVenta = 89.99m,
                MinimoExistencia = 15,
                Eliminado = false,
                FechaCreacion = now,
                UltimaFechaActualizacion = now
            }, 30)
        };

        var existingCodes = (await context.Productos
            .IgnoreQueryFilters()
            .Select(p => p.CodigoProducto)
            .ToListAsync())
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var newProducts = blueprint
            .Where(b => !existingCodes.Contains(b.Producto.CodigoProducto))
            .ToList();

        if (newProducts.Count == 0)
        {
            return;
        }

        await context.Productos.AddRangeAsync(newProducts.Select(b => b.Producto));
        await context.SaveChangesAsync();

        var inventarios = newProducts
            .Select(b => new Inventario
            {
                IdProducto = b.Producto.IdProducto,
                Existencia = b.Stock,
                UltimaFechaActualizacion = now
            })
            .ToList();

        await context.Inventarios.AddRangeAsync(inventarios);
        await context.SaveChangesAsync();
    }
}
