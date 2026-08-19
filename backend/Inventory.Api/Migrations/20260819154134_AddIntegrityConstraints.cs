using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Inventory.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddIntegrityConstraints : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddCheckConstraint(
                name: "CK_Products_Balance_NonNegative",
                table: "Products",
                sql: "\"Balance\" >= 0");

            migrationBuilder.CreateIndex(
                name: "IX_IdempotencyRecords_ProcessedDate",
                table: "IdempotencyRecords",
                column: "ProcessedDate");

            migrationBuilder.CreateIndex(
                name: "IX_IdempotencyRecords_ProductId",
                table: "IdempotencyRecords",
                column: "ProductId");

            migrationBuilder.AddForeignKey(
                name: "FK_IdempotencyRecords_Products_ProductId",
                table: "IdempotencyRecords",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_IdempotencyRecords_Products_ProductId",
                table: "IdempotencyRecords");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Products_Balance_NonNegative",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_IdempotencyRecords_ProcessedDate",
                table: "IdempotencyRecords");

            migrationBuilder.DropIndex(
                name: "IX_IdempotencyRecords_ProductId",
                table: "IdempotencyRecords");
        }
    }
}
