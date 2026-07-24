/*
    Standalone migration for atomic inventory reservation.

    Target database: CloneEbayDB (SQL Server)
    Safe to run more than once.

    Changes:
      - Creates dbo.InventoryReservation.
      - Merges duplicate Inventory rows by product.
      - Enforces one Inventory row per product.
      - Prevents negative stock and invalid reservation data.
      - Registers the matching EF Core migrations.

    This file intentionally contains only the inventory-reservation migration.
    It does not create or seed the rest of the application database.
*/

IF DB_ID(N'CloneEbayDB') IS NULL
BEGIN
    THROW 51000, 'CloneEbayDB does not exist. Create the base database before running this migration.', 1;
END;
GO

USE [CloneEbayDB];
GO

SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
SET XACT_ABORT ON;
GO

BEGIN TRY
    BEGIN TRANSACTION;

    IF OBJECT_ID(N'dbo.Inventory', N'U') IS NULL
        THROW 51001, 'Required table dbo.Inventory does not exist.', 1;

    IF OBJECT_ID(N'dbo.OrderTable', N'U') IS NULL
        THROW 51002, 'Required table dbo.OrderTable does not exist.', 1;

    IF OBJECT_ID(N'dbo.Product', N'U') IS NULL
        THROW 51003, 'Required table dbo.Product does not exist.', 1;

    IF OBJECT_ID(N'dbo.__EFMigrationsHistory', N'U') IS NULL
    BEGIN
        CREATE TABLE [dbo].[__EFMigrationsHistory]
        (
            [MigrationId] nvarchar(150) NOT NULL,
            [ProductVersion] nvarchar(32) NOT NULL,
            CONSTRAINT [PK___EFMigrationsHistory]
                PRIMARY KEY ([MigrationId])
        );
    END;

    IF OBJECT_ID(N'dbo.InventoryReservation', N'U') IS NULL
    BEGIN
        CREATE TABLE [dbo].[InventoryReservation]
        (
            [id] int IDENTITY(1,1) NOT NULL,
            [orderId] int NOT NULL,
            [productId] int NOT NULL,
            [quantity] int NOT NULL,
            [status] nvarchar(20) NOT NULL,
            [createdAt] datetime NOT NULL,
            [confirmedAt] datetime NULL,
            [releasedAt] datetime NULL,
            CONSTRAINT [PK_InventoryReservation]
                PRIMARY KEY ([id])
        );
    END;

    IF EXISTS
    (
        SELECT 1
        FROM [dbo].[Inventory]
        WHERE [quantity] < 0
    )
    BEGIN
        THROW 51004, 'Inventory contains negative quantities. Correct the data before running this migration.', 1;
    END;

    /* Preserve total stock while reducing Inventory to one row per product. */
    ;WITH InventoryTotals AS
    (
        SELECT
            [productId],
            MIN([id]) AS [keeperId],
            SUM(ISNULL([quantity], 0)) AS [totalQuantity]
        FROM [dbo].[Inventory]
        WHERE [productId] IS NOT NULL
        GROUP BY [productId]
        HAVING COUNT(*) > 1
    )
    UPDATE inventory
    SET
        inventory.[quantity] = totals.[totalQuantity],
        inventory.[lastUpdated] = GETUTCDATE()
    FROM [dbo].[Inventory] AS inventory
    INNER JOIN InventoryTotals AS totals
        ON totals.[keeperId] = inventory.[id];

    ;WITH DuplicateInventory AS
    (
        SELECT
            [id],
            ROW_NUMBER() OVER
            (
                PARTITION BY [productId]
                ORDER BY [id]
            ) AS [rowNumber]
        FROM [dbo].[Inventory]
        WHERE [productId] IS NOT NULL
    )
    DELETE FROM DuplicateInventory
    WHERE [rowNumber] > 1;

    IF NOT EXISTS
    (
        SELECT 1
        FROM sys.check_constraints
        WHERE [name] = N'CK_Inventory_Quantity_NonNegative'
          AND [parent_object_id] = OBJECT_ID(N'dbo.Inventory')
    )
    BEGIN
        ALTER TABLE [dbo].[Inventory] WITH CHECK
            ADD CONSTRAINT [CK_Inventory_Quantity_NonNegative]
            CHECK ([quantity] IS NULL OR [quantity] >= 0);
    END;

    IF NOT EXISTS
    (
        SELECT 1
        FROM sys.check_constraints
        WHERE [name] = N'CK_InventoryReservation_Quantity_Positive'
          AND [parent_object_id] = OBJECT_ID(N'dbo.InventoryReservation')
    )
    BEGIN
        ALTER TABLE [dbo].[InventoryReservation] WITH CHECK
            ADD CONSTRAINT [CK_InventoryReservation_Quantity_Positive]
            CHECK ([quantity] > 0);
    END;

    IF NOT EXISTS
    (
        SELECT 1
        FROM sys.check_constraints
        WHERE [name] = N'CK_InventoryReservation_Status'
          AND [parent_object_id] = OBJECT_ID(N'dbo.InventoryReservation')
    )
    BEGIN
        ALTER TABLE [dbo].[InventoryReservation] WITH CHECK
            ADD CONSTRAINT [CK_InventoryReservation_Status]
            CHECK ([status] IN (N'Held', N'Confirmed', N'Released'));
    END;

    IF EXISTS
    (
        SELECT 1
        FROM sys.indexes
        WHERE [name] = N'IX_Inventory_productId'
          AND [object_id] = OBJECT_ID(N'dbo.Inventory')
          AND
          (
              [is_unique] = 0
              OR [has_filter] = 0
          )
    )
    BEGIN
        DROP INDEX [IX_Inventory_productId]
            ON [dbo].[Inventory];
    END;

    IF NOT EXISTS
    (
        SELECT 1
        FROM sys.indexes
        WHERE [name] = N'IX_Inventory_productId'
          AND [object_id] = OBJECT_ID(N'dbo.Inventory')
    )
    BEGIN
        CREATE UNIQUE INDEX [IX_Inventory_productId]
            ON [dbo].[Inventory] ([productId])
            WHERE [productId] IS NOT NULL;
    END;

    IF NOT EXISTS
    (
        SELECT 1
        FROM sys.indexes
        WHERE [name] = N'IX_InventoryReservation_orderId_productId'
          AND [object_id] = OBJECT_ID(N'dbo.InventoryReservation')
    )
    BEGIN
        CREATE UNIQUE INDEX [IX_InventoryReservation_orderId_productId]
            ON [dbo].[InventoryReservation] ([orderId], [productId]);
    END;

    IF NOT EXISTS
    (
        SELECT 1
        FROM sys.indexes
        WHERE [name] = N'IX_InventoryReservation_productId'
          AND [object_id] = OBJECT_ID(N'dbo.InventoryReservation')
    )
    BEGIN
        CREATE INDEX [IX_InventoryReservation_productId]
            ON [dbo].[InventoryReservation] ([productId]);
    END;

    IF NOT EXISTS
    (
        SELECT 1
        FROM sys.foreign_keys
        WHERE [name] = N'FK_InventoryReservation_OrderTable_orderId'
          AND [parent_object_id] = OBJECT_ID(N'dbo.InventoryReservation')
    )
    BEGIN
        ALTER TABLE [dbo].[InventoryReservation] WITH CHECK
            ADD CONSTRAINT [FK_InventoryReservation_OrderTable_orderId]
            FOREIGN KEY ([orderId])
            REFERENCES [dbo].[OrderTable] ([id])
            ON DELETE CASCADE;
    END;

    IF NOT EXISTS
    (
        SELECT 1
        FROM sys.foreign_keys
        WHERE [name] = N'FK_InventoryReservation_Product_productId'
          AND [parent_object_id] = OBJECT_ID(N'dbo.InventoryReservation')
    )
    BEGIN
        ALTER TABLE [dbo].[InventoryReservation] WITH CHECK
            ADD CONSTRAINT [FK_InventoryReservation_Product_productId]
            FOREIGN KEY ([productId])
            REFERENCES [dbo].[Product] ([id]);
    END;

    IF NOT EXISTS
    (
        SELECT 1
        FROM [dbo].[__EFMigrationsHistory]
        WHERE [MigrationId] = N'20260724152041_AddInventoryReservations'
    )
    BEGIN
        INSERT INTO [dbo].[__EFMigrationsHistory]
            ([MigrationId], [ProductVersion])
        VALUES
            (N'20260724152041_AddInventoryReservations', N'8.0.6');
    END;

    IF NOT EXISTS
    (
        SELECT 1
        FROM [dbo].[__EFMigrationsHistory]
        WHERE [MigrationId] = N'20260724152347_EnforceInventoryIntegrity'
    )
    BEGIN
        INSERT INTO [dbo].[__EFMigrationsHistory]
            ([MigrationId], [ProductVersion])
        VALUES
            (N'20260724152347_EnforceInventoryIntegrity', N'8.0.6');
    END;

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF XACT_STATE() <> 0
        ROLLBACK TRANSACTION;

    THROW;
END CATCH;
GO

SELECT
    N'Inventory reservation migration completed successfully.' AS [Result],
    DB_NAME() AS [DatabaseName],
    OBJECT_ID(N'dbo.InventoryReservation', N'U') AS [ReservationTableObjectId];
GO
