using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class EnforceInventoryIntegrity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                ;WITH InventoryTotals AS
                (
                    SELECT productId, MIN(id) AS keeperId, SUM(ISNULL(quantity, 0)) AS totalQuantity
                    FROM Inventory
                    WHERE productId IS NOT NULL
                    GROUP BY productId
                    HAVING COUNT(*) > 1
                )
                UPDATE inventory
                SET quantity = totals.totalQuantity,
                    lastUpdated = GETUTCDATE()
                FROM Inventory AS inventory
                INNER JOIN InventoryTotals AS totals ON totals.keeperId = inventory.id;

                ;WITH DuplicateInventory AS
                (
                    SELECT id,
                           ROW_NUMBER() OVER (PARTITION BY productId ORDER BY id) AS rowNumber
                    FROM Inventory
                    WHERE productId IS NOT NULL
                )
                DELETE FROM DuplicateInventory
                WHERE rowNumber > 1;
                """);

            migrationBuilder.DropIndex(
                name: "IX_Inventory_productId",
                table: "Inventory");

            migrationBuilder.AddCheckConstraint(
                name: "CK_InventoryReservation_Quantity_Positive",
                table: "InventoryReservation",
                sql: "[quantity] > 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_InventoryReservation_Status",
                table: "InventoryReservation",
                sql: "[status] IN ('Held', 'Confirmed', 'Released')");

            migrationBuilder.CreateIndex(
                name: "IX_Inventory_productId",
                table: "Inventory",
                column: "productId",
                unique: true,
                filter: "[productId] IS NOT NULL");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Inventory_Quantity_NonNegative",
                table: "Inventory",
                sql: "[quantity] IS NULL OR [quantity] >= 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_InventoryReservation_Quantity_Positive",
                table: "InventoryReservation");

            migrationBuilder.DropCheckConstraint(
                name: "CK_InventoryReservation_Status",
                table: "InventoryReservation");

            migrationBuilder.DropIndex(
                name: "IX_Inventory_productId",
                table: "Inventory");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Inventory_Quantity_NonNegative",
                table: "Inventory");

            migrationBuilder.CreateIndex(
                name: "IX_Inventory_productId",
                table: "Inventory",
                column: "productId");
        }
    }
}
