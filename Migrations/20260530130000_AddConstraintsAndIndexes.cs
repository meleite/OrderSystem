using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OrderSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddConstraintsAndIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Unique constraint on Users.Email: enforces uniqueness at the DB level
            // and speeds up login lookups by email.
            migrationBuilder.CreateIndex(
                name: "UX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);

            // Drop the plain FK-auto-generated index on Orders.UserId.
            // It is fully superseded by the composite (UserId, CreatedAt) below:
            // SQL Server can use a composite index to satisfy lookups on its leftmost column.
            migrationBuilder.DropIndex(
                name: "IX_Orders_UserId",
                table: "Orders");

            // Composite (UserId, CreatedAt): covers "my orders" queries with date filtering
            // and sorting, as well as plain FK lookups on UserId alone.
            migrationBuilder.CreateIndex(
                name: "IX_Orders_UserId_CreatedAt",
                table: "Orders",
                columns: new[] { "UserId", "CreatedAt" });

            // Standalone CreatedAt: supports admin/reporting queries that scan orders
            // across all users by date range (e.g. daily/monthly revenue reports).
            migrationBuilder.CreateIndex(
                name: "IX_Orders_CreatedAt",
                table: "Orders",
                column: "CreatedAt");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Orders_CreatedAt",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Orders_UserId_CreatedAt",
                table: "Orders");

            // Restore the original FK auto-index.
            migrationBuilder.CreateIndex(
                name: "IX_Orders_UserId",
                table: "Orders",
                column: "UserId");

            migrationBuilder.DropIndex(
                name: "UX_Users_Email",
                table: "Users");
        }
    }
}
