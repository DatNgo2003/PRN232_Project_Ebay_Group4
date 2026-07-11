using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddOrderPricingColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ===== Coupon: thêm usedCount =====
            migrationBuilder.AddColumn<int>(
                name: "usedCount",
                table: "Coupon",
                type: "int",
                nullable: true,
                defaultValue: 0);

            // ===== OrderTable: thêm subTotal, shippingFee, discountAmount, couponId =====
            migrationBuilder.AddColumn<decimal>(
                name: "subTotal",
                table: "OrderTable",
                type: "decimal(10,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "shippingFee",
                table: "OrderTable",
                type: "decimal(10,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "discountAmount",
                table: "OrderTable",
                type: "decimal(10,2)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "couponId",
                table: "OrderTable",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrderTable_couponId",
                table: "OrderTable",
                column: "couponId");

            migrationBuilder.AddForeignKey(
                name: "FK_OrderTable_couponId",
                table: "OrderTable",
                column: "couponId",
                principalTable: "Coupon",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrderTable_couponId",
                table: "OrderTable");

            migrationBuilder.DropIndex(
                name: "IX_OrderTable_couponId",
                table: "OrderTable");

            migrationBuilder.DropColumn(
                name: "couponId",
                table: "OrderTable");

            migrationBuilder.DropColumn(
                name: "discountAmount",
                table: "OrderTable");

            migrationBuilder.DropColumn(
                name: "shippingFee",
                table: "OrderTable");

            migrationBuilder.DropColumn(
                name: "subTotal",
                table: "OrderTable");

            migrationBuilder.DropColumn(
                name: "usedCount",
                table: "Coupon");
        }
    }
}