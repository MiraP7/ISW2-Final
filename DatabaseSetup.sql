-- Crear base de datos
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'InventoryDb')
BEGIN
    CREATE DATABASE InventoryDb;
END
GO

USE InventoryDb;
GO

-- Tabla ApiKeys (para autenticación)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ApiKeys')
BEGIN
    CREATE TABLE ApiKeys (
        IdApiKey INT IDENTITY(1,1) PRIMARY KEY,
        Clave NVARCHAR(100) NOT NULL UNIQUE,
        Nombre NVARCHAR(100) NOT NULL,
        Descripcion NVARCHAR(255),
        Activa BIT NOT NULL DEFAULT 1,
        FechaCreacion DATETIME DEFAULT GETUTCDATE(),
        FechaVencimiento DATETIME NULL,
        UltimaFechaUso DATETIME DEFAULT GETUTCDATE()
    );
END
GO

-- Tabla TipoMovimiento
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'TipoMovimiento')
BEGIN
    CREATE TABLE TipoMovimiento (
        IdTipoMovimiento INT PRIMARY KEY,
        Tipo NVARCHAR(50) NOT NULL
    );
    INSERT INTO TipoMovimiento VALUES (1, 'Entrada'), (2, 'Salida');
END
GO

-- Tabla Productos (con CodigoProducto y Eliminado)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Productos')
BEGIN
    CREATE TABLE Productos (
        IdProducto INT IDENTITY(1,1) PRIMARY KEY,
        Nombre NVARCHAR(100) NOT NULL,
        CodigoProducto NVARCHAR(50) NOT NULL UNIQUE,
        Descripcion NVARCHAR(255),
        PrecioVenta DECIMAL(18,2) NOT NULL,
        MinimoExistencia INT NOT NULL DEFAULT 0,
        Eliminado BIT NOT NULL DEFAULT 0,
        FechaCreacion DATETIME DEFAULT GETUTCDATE(),
        UltimaFechaActualizacion DATETIME DEFAULT GETUTCDATE()
    );
END
GO

-- Tabla Inventario (con ON DELETE RESTRICT)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Inventario')
BEGIN
    CREATE TABLE Inventario (
        IdProducto INT PRIMARY KEY,
        Existencia INT NOT NULL DEFAULT 0,
        UltimaFechaActualizacion DATETIME DEFAULT GETUTCDATE(),
        CONSTRAINT FK_Inventario_Producto FOREIGN KEY (IdProducto) 
            REFERENCES Productos(IdProducto) ON DELETE NO ACTION ON UPDATE CASCADE
    );
END
GO

-- Tabla MovimientosInventario (con ON DELETE RESTRICT)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'MovimientosInventario')
BEGIN
    CREATE TABLE MovimientosInventario (
        IdMovimiento INT IDENTITY(1,1) PRIMARY KEY,
        IdProductoAsociado INT NOT NULL,
        Fecha DATETIME NOT NULL,
        Cantidad INT NOT NULL,
        IdTipoMovimiento INT NOT NULL,
        UltimaFechaActualizacion DATETIME DEFAULT GETUTCDATE(),
        CONSTRAINT FK_MovimientosInventario_Producto FOREIGN KEY (IdProductoAsociado) 
            REFERENCES Productos(IdProducto) ON DELETE NO ACTION ON UPDATE CASCADE,
        CONSTRAINT FK_MovimientosInventario_TipoMovimiento FOREIGN KEY (IdTipoMovimiento) 
            REFERENCES TipoMovimiento(IdTipoMovimiento) ON DELETE NO ACTION ON UPDATE CASCADE
    );
END
GO

-- =====================================================
-- STORED PROCEDURES para Productos
-- =====================================================

-- SP: GetProductos (solo no eliminados)
IF EXISTS (SELECT * FROM sys.procedures WHERE name = 'GetProductos')
    DROP PROCEDURE GetProductos;
GO
CREATE PROCEDURE GetProductos
AS
BEGIN
    SET NOCOUNT ON;
    SELECT IdProducto, Nombre, CodigoProducto, Descripcion, PrecioVenta, MinimoExistencia, Eliminado, FechaCreacion, UltimaFechaActualizacion
    FROM Productos
    WHERE Eliminado = 0;
END
GO

-- SP: GetProducto
IF EXISTS (SELECT * FROM sys.procedures WHERE name = 'GetProducto')
    DROP PROCEDURE GetProducto;
GO
CREATE PROCEDURE GetProducto @IdProducto INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT IdProducto, Nombre, CodigoProducto, Descripcion, PrecioVenta, MinimoExistencia, Eliminado, FechaCreacion, UltimaFechaActualizacion
    FROM Productos
    WHERE IdProducto = @IdProducto AND Eliminado = 0;
END
GO

-- SP: InsertProducto (genera CodigoProducto automáticamente)
IF EXISTS (SELECT * FROM sys.procedures WHERE name = 'InsertProducto')
    DROP PROCEDURE InsertProducto;
GO
CREATE PROCEDURE InsertProducto 
    @Nombre NVARCHAR(100), 
    @Descripcion NVARCHAR(255), 
    @PrecioVenta DECIMAL(18,2), 
    @MinimoExistencia INT,
    @IdProducto INT OUTPUT,
    @CodigoProducto NVARCHAR(50) OUTPUT,
    @Mensaje NVARCHAR(255) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @NextId INT;
    DECLARE @GeneratedCode NVARCHAR(50);
    
    -- Obtener el siguiente ID disponible
    SELECT @NextId = ISNULL(MAX(IdProducto), 0) + 1 FROM Productos;
    
    -- Generar código con formato PROD-XXXX (4 dígitos con ceros a la izquierda)
    SET @GeneratedCode = 'PROD-' + RIGHT('0000' + CAST(@NextId AS NVARCHAR), 4);
    
    -- Insertar el producto
    INSERT INTO Productos (Nombre, CodigoProducto, Descripcion, PrecioVenta, MinimoExistencia, Eliminado, FechaCreacion, UltimaFechaActualizacion)
    VALUES (@Nombre, @GeneratedCode, @Descripcion, @PrecioVenta, @MinimoExistencia, 0, GETUTCDATE(), GETUTCDATE());
    
    SET @IdProducto = SCOPE_IDENTITY();
    SET @CodigoProducto = @GeneratedCode;
    SET @Mensaje = 'Producto creado exitosamente con código ' + @GeneratedCode;
END
GO

-- SP: UpdateProducto (permite actualizar CodigoProducto si es único)
IF EXISTS (SELECT * FROM sys.procedures WHERE name = 'UpdateProducto')
    DROP PROCEDURE UpdateProducto;
GO
CREATE PROCEDURE UpdateProducto 
    @IdProducto INT, 
    @Nombre NVARCHAR(100), 
    @CodigoProducto NVARCHAR(50),
    @Descripcion NVARCHAR(255), 
    @PrecioVenta DECIMAL(18,2), 
    @MinimoExistencia INT,
    @Mensaje NVARCHAR(255) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Verificar que el producto existe y no está eliminado
    IF NOT EXISTS (SELECT 1 FROM Productos WHERE IdProducto = @IdProducto AND Eliminado = 0)
    BEGIN
        SET @Mensaje = 'Producto no encontrado.';
        RETURN;
    END
    
    -- Lógica para CodigoProducto:
    -- Si el código enviado es diferente al actual, verificar si ya existe.
    -- Si existe, NO actualizar el código (pero sí el resto).
    -- Si no existe, actualizar todo.
    
    DECLARE @CodigoActual NVARCHAR(50);
    DECLARE @UpdateCode BIT = 0;
    
    SELECT @CodigoActual = CodigoProducto FROM Productos WHERE IdProducto = @IdProducto;
    
    -- Si el código es diferente y no es nulo/vacío
    IF @CodigoProducto IS NOT NULL AND @CodigoProducto <> '' AND @CodigoProducto <> @CodigoActual
    BEGIN
        IF EXISTS (SELECT 1 FROM Productos WHERE CodigoProducto = @CodigoProducto AND IdProducto <> @IdProducto)
        BEGIN
            -- El código ya existe en otro producto. No lo actualizamos.
            SET @Mensaje = 'Producto actualizado, pero el código no se modificó porque ya existe.';
        END
        ELSE
        BEGIN
            -- El código es único. Lo actualizamos.
            SET @UpdateCode = 1;
            SET @Mensaje = 'Producto actualizado exitosamente (incluyendo código).';
        END
    END
    ELSE
    BEGIN
        SET @Mensaje = 'Producto actualizado exitosamente.';
    END
    
    -- Actualizar campos
    UPDATE Productos
    SET Nombre = @Nombre,
        CodigoProducto = CASE WHEN @UpdateCode = 1 THEN @CodigoProducto ELSE CodigoProducto END,
        Descripcion = @Descripcion,
        PrecioVenta = @PrecioVenta,
        MinimoExistencia = @MinimoExistencia,
        UltimaFechaActualizacion = GETUTCDATE()
    WHERE IdProducto = @IdProducto;
END
GO

-- SP: DeleteProducto (borrado lógico con validaciones de integridad)
IF EXISTS (SELECT * FROM sys.procedures WHERE name = 'DeleteProducto')
    DROP PROCEDURE DeleteProducto;
GO
CREATE PROCEDURE DeleteProducto 
    @IdProducto INT,
    @Mensaje NVARCHAR(255) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Verificar que el producto existe
    IF NOT EXISTS (SELECT 1 FROM Productos WHERE IdProducto = @IdProducto)
    BEGIN
        SET @Mensaje = 'Producto no encontrado.';
        RETURN;
    END
    
    -- RESTRICCIÓN 1: Verificar si existe en Inventario
    IF EXISTS (SELECT 1 FROM Inventario WHERE IdProducto = @IdProducto)
    BEGIN
        SET @Mensaje = 'No se puede eliminar el producto porque tiene registros de inventario asociados.';
        RETURN;
    END
    
    -- RESTRICCIÓN 2: Verificar si existe en MovimientosInventario
    IF EXISTS (SELECT 1 FROM MovimientosInventario WHERE IdProductoAsociado = @IdProducto)
    BEGIN
        SET @Mensaje = 'No se puede eliminar el producto porque tiene movimientos de inventario asociados.';
        RETURN;
    END
    
    -- Si no hay restricciones, proceder con el borrado lógico
    UPDATE Productos 
    SET Eliminado = 1, 
        UltimaFechaActualizacion = GETUTCDATE()
    WHERE IdProducto = @IdProducto;
    
    SET @Mensaje = 'Producto eliminado exitosamente (borrado lógico).';
END
GO

-- =====================================================
-- STORED PROCEDURES para Inventario
-- =====================================================

-- SP: GetInventario
IF EXISTS (SELECT * FROM sys.procedures WHERE name = 'GetInventario')
    DROP PROCEDURE GetInventario;
GO
CREATE PROCEDURE GetInventario
AS
BEGIN
    SET NOCOUNT ON;
    SELECT 
        i.IdProducto,
        p.Nombre AS Producto,
        i.Existencia,
        p.MinimoExistencia,
        CASE 
            WHEN i.Existencia > p.MinimoExistencia THEN 'Normal'
            WHEN i.Existencia = p.MinimoExistencia THEN 'En Mínimo'
            ELSE 'Bajo Mínimo'
        END AS Estado,
        i.UltimaFechaActualizacion
    FROM Inventario i
    INNER JOIN Productos p ON i.IdProducto = p.IdProducto
    WHERE p.Eliminado = 0;
END
GO

-- SP: GetInventarioByProducto
IF EXISTS (SELECT * FROM sys.procedures WHERE name = 'GetInventarioByProducto')
    DROP PROCEDURE GetInventarioByProducto;
GO
CREATE PROCEDURE GetInventarioByProducto @IdProducto INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT 
        i.IdProducto,
        p.Nombre AS Producto,
        i.Existencia,
        p.MinimoExistencia,
        CASE 
            WHEN i.Existencia > p.MinimoExistencia THEN 'Normal'
            WHEN i.Existencia = p.MinimoExistencia THEN 'En Mínimo'
            ELSE 'Bajo Mínimo'
        END AS Estado,
        i.UltimaFechaActualizacion
    FROM Inventario i
    INNER JOIN Productos p ON i.IdProducto = p.IdProducto
    WHERE i.IdProducto = @IdProducto AND p.Eliminado = 0;
END
GO

-- =====================================================
-- DATA SEED: API Keys de prueba
-- =====================================================

-- Insertar API Key de prueba
IF NOT EXISTS (SELECT * FROM ApiKeys WHERE Nombre = 'API Key Test')
BEGIN
    INSERT INTO ApiKeys (Clave, Nombre, Descripcion, Activa, FechaVencimiento)
    VALUES 
        ('sk_test_12345678901234567890123456789012', 'API Key Test', 'Para pruebas en Swagger', 1, NULL),
        ('sk_prod_abcdefghijklmnopqrstuvwxyz123456', 'API Key Producción', 'Para uso en producción', 1, NULL);
END
GO