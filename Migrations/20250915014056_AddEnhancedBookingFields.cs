using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Car_Rent_System.Migrations
{
    /// <inheritdoc />
    public partial class AddEnhancedBookingFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "PerKilometerRate",
                table: "Cars",
                type: "decimal(10,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "AdvancePayment",
                table: "Bookings",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "DistanceTraveled",
                table: "Bookings",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DocumentImageUrl",
                table: "Bookings",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LicenseNumber",
                table: "Bookings",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NICNumber",
                table: "Bookings",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "OdometerEndReading",
                table: "Bookings",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "OdometerStartReading",
                table: "Bookings",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "PerKilometerRate",
                table: "Bookings",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "RemainingAmount",
                table: "Bookings",
                type: "decimal(18,2)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PerKilometerRate",
                table: "Cars");

            migrationBuilder.DropColumn(
                name: "AdvancePayment",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "DistanceTraveled",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "DocumentImageUrl",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "LicenseNumber",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "NICNumber",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "OdometerEndReading",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "OdometerStartReading",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "PerKilometerRate",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "RemainingAmount",
                table: "Bookings");
        }
    }
}
