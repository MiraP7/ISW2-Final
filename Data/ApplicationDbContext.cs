using Microsoft.EntityFrameworkCore;
using ISW2_Primer_parcial.Models;

namespace ISW2_Primer_parcial.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<Producto> Productos { get; set; }
    public DbSet<Inventario> Inventarios { get; set; }
    public DbSet<MovimientosInventario> MovimientosInventario { get; set; }
    public DbSet<TipoMovimiento> TipoMovimientos { get; set; }
    public DbSet<ApiKey> ApiKeys { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Configure table names
        modelBuilder.Entity<Producto>().ToTable("Productos");
        modelBuilder.Entity<Inventario>().ToTable("Inventario");
        modelBuilder.Entity<MovimientosInventario>().ToTable("MovimientosInventario");
        modelBuilder.Entity<TipoMovimiento>().ToTable("TipoMovimiento");
        modelBuilder.Entity<ApiKey>().ToTable("ApiKeys");

        modelBuilder.Entity<Producto>()
            .HasIndex(p => p.CodigoProducto)
            .IsUnique();

        modelBuilder.Entity<Producto>()
            .HasQueryFilter(p => !p.Eliminado);

        modelBuilder.Entity<ApiKey>()
            .HasIndex(a => a.Clave)
            .IsUnique();
        
        // Configure relationships
        modelBuilder.Entity<Inventario>()
            .HasOne(i => i.Producto)
            .WithMany()
            .HasForeignKey(i => i.IdProducto);

        modelBuilder.Entity<MovimientosInventario>()
            .HasOne(m => m.Producto)
            .WithMany()
            .HasForeignKey(m => m.IdProductoAsociado);

        modelBuilder.Entity<MovimientosInventario>()
            .HasOne(m => m.TipoMovimiento)
            .WithMany()
            .HasForeignKey(m => m.IdTipoMovimiento);

        // Configure decimal precision
        modelBuilder.Entity<Producto>()
            .Property(p => p.PrecioVenta)
            .HasColumnType("decimal(18,2)");

        // Seed TipoMovimiento
        modelBuilder.Entity<TipoMovimiento>().HasData(
            new TipoMovimiento { IdTipoMovimiento = 1, Tipo = "Entrada" },
            new TipoMovimiento { IdTipoMovimiento = 2, Tipo = "Salida" }
        );
    }
}