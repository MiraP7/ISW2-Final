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
    public DbSet<Usuario> Usuarios { get; set; }
    public DbSet<Rol> Roles { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Configure table names
        modelBuilder.Entity<Producto>().ToTable("Productos");
        modelBuilder.Entity<Inventario>().ToTable("Inventario");
        modelBuilder.Entity<MovimientosInventario>().ToTable("MovimientosInventario");
        modelBuilder.Entity<TipoMovimiento>().ToTable("TipoMovimiento");
        modelBuilder.Entity<ApiKey>().ToTable("ApiKeys");
        modelBuilder.Entity<Usuario>().ToTable("Usuarios");
        modelBuilder.Entity<Rol>().ToTable("Roles");

        // Configurar Usuario
        modelBuilder.Entity<Usuario>()
            .HasIndex(u => u.Email)
            .IsUnique();

        modelBuilder.Entity<Usuario>()
            .HasIndex(u => u.NombreUsuario)
            .IsUnique();

        modelBuilder.Entity<Usuario>()
            .HasOne(u => u.Rol)
            .WithMany(r => r.Usuarios)
            .HasForeignKey(u => u.IdRol);

        // Configurar Rol
        modelBuilder.Entity<Rol>()
            .HasIndex(r => r.Nombre)
            .IsUnique();

        modelBuilder.Entity<Producto>()
            .HasIndex(p => p.CodigoProducto)
            .IsUnique();

        modelBuilder.Entity<Producto>()
            .HasQueryFilter(p => !p.Eliminado);

        // Agregar filtros de consulta a entidades relacionadas para evitar warnings
        modelBuilder.Entity<Inventario>()
            .HasQueryFilter(i => i.Producto != null && !i.Producto.Eliminado);

        modelBuilder.Entity<MovimientosInventario>()
            .HasQueryFilter(m => m.Producto != null && !m.Producto.Eliminado);

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

        // Seed Roles
        modelBuilder.Entity<Rol>().HasData(
            new Rol { IdRol = 1, Nombre = "Administrador", Descripcion = "Acceso completo al sistema" },
            new Rol { IdRol = 2, Nombre = "Usuario", Descripcion = "Acceso limitado, no puede eliminar productos" }
        );
    }
}