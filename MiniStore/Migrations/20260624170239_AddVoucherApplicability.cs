using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MiniStore.Migrations
{
    /// <inheritdoc />
    public partial class AddVoucherApplicability : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ApplicableCategoryId",
                table: "Vouchers",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ApplicableProductIds",
                table: "Vouchers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ApplicableType",
                table: "Vouchers",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ApplicableCategoryId",
                table: "Vouchers");

            migrationBuilder.DropColumn(
                name: "ApplicableProductIds",
                table: "Vouchers");

            migrationBuilder.DropColumn(
                name: "ApplicableType",
                table: "Vouchers");
        }
    }
}
