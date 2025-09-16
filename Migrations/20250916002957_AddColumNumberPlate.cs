using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Car_Rent_System.Migrations
{
    /// <inheritdoc />
    public partial class AddColumNumberPlate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Payments_Bookings_BookingId",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "PaymentMethod",
                table: "Payments");

            migrationBuilder.AddColumn<int>(
                name: "PaymentMethodId",
                table: "Payments",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "ProcessedAt",
                table: "Payments",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NumberPlate",
                table: "Cars",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "LicenseImageBackUrl",
                table: "Bookings",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LicenseImageFrontUrl",
                table: "Bookings",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "PaymentMethods",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentMethods", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "PaymentMethods",
                columns: new[] { "Id", "CreatedAt", "Description", "IsActive", "Name" },
                values: new object[,]
                {
                    { 1, new DateTime(2025, 9, 16, 0, 29, 54, 984, DateTimeKind.Utc).AddTicks(1977), "Cash payment", true, "Cash" },
                    { 2, new DateTime(2025, 9, 16, 0, 29, 54, 984, DateTimeKind.Utc).AddTicks(1984), "Credit card payment", true, "Credit Card" },
                    { 3, new DateTime(2025, 9, 16, 0, 29, 54, 984, DateTimeKind.Utc).AddTicks(1988), "Bank transfer payment", true, "Bank Transfer" },
                    { 4, new DateTime(2025, 9, 16, 0, 29, 54, 984, DateTimeKind.Utc).AddTicks(1993), "Stripe payment gateway", true, "Stripe" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Payments_PaymentMethodId",
                table: "Payments",
                column: "PaymentMethodId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentMethods_Name",
                table: "PaymentMethods",
                column: "Name",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_Bookings_BookingId",
                table: "Payments",
                column: "BookingId",
                principalTable: "Bookings",
                principalColumn: "BookingID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_PaymentMethods_PaymentMethodId",
                table: "Payments",
                column: "PaymentMethodId",
                principalTable: "PaymentMethods",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Payments_Bookings_BookingId",
                table: "Payments");

            migrationBuilder.DropForeignKey(
                name: "FK_Payments_PaymentMethods_PaymentMethodId",
                table: "Payments");

            migrationBuilder.DropTable(
                name: "PaymentMethods");

            migrationBuilder.DropIndex(
                name: "IX_Payments_PaymentMethodId",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "PaymentMethodId",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "ProcessedAt",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "NumberPlate",
                table: "Cars");

            migrationBuilder.DropColumn(
                name: "LicenseImageBackUrl",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "LicenseImageFrontUrl",
                table: "Bookings");

            migrationBuilder.AddColumn<string>(
                name: "PaymentMethod",
                table: "Payments",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_Bookings_BookingId",
                table: "Payments",
                column: "BookingId",
                principalTable: "Bookings",
                principalColumn: "BookingID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
