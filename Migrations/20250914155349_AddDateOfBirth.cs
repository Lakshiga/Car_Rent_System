using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Car_Rent_System.Migrations
{
    /// <inheritdoc />
    public partial class AddDateOfBirth : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "StripeSessionId",
                table: "Payments",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DateOfBirth",
                table: "AspNetUsers",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StripeSessionId",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "DateOfBirth",
                table: "AspNetUsers");
        }
    }
}
