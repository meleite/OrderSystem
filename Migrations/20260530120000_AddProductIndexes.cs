using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OrderSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddProductIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Cap Category to nvarchar(100) so it can be included in a composite index
            // (SQL Server does not allow indexing nvarchar(max) columns).
            migrationBuilder.AlterColumn<string>(
                name: "Category",
                table: "Products",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            // Supports ORDER BY Name and name-prefix searches.
            migrationBuilder.CreateIndex(
                name: "IX_Products_Name",
                table: "Products",
                column: "Name");

            // Covers the most common public filter: active products in a category.
            migrationBuilder.CreateIndex(
                name: "IX_Products_IsActive_Category",
                table: "Products",
                columns: new[] { "IsActive", "Category" });

            // Covers price-range filters on active products.
            migrationBuilder.CreateIndex(
                name: "IX_Products_IsActive_Price",
                table: "Products",
                columns: new[] { "IsActive", "Price" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Products_IsActive_Price",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Products_IsActive_Category",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Products_Name",
                table: "Products");

            migrationBuilder.AlterColumn<string>(
                name: "Category",
                table: "Products",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);
        }
    }
}
