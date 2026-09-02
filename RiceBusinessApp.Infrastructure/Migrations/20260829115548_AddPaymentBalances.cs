using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RiceBusinessApp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPaymentBalances : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "NewBalance",
                table: "Payments",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "PreviousBalance",
                table: "Payments",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NewBalance",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "PreviousBalance",
                table: "Payments");
        }
    }
}
