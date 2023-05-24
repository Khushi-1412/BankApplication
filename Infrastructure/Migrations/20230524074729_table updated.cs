using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BankApplication.Infrastructure.Migrations
{
    public partial class tableupdated : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Balance",
                table: "Bank");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "Bank",
                newName: "Gender");

            migrationBuilder.AddColumn<decimal>(
                name: "AccountBalance",
                table: "Bank",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "AccountHolderName",
                table: "Bank",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "DateOfBirth",
                table: "Bank",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AccountBalance",
                table: "Bank");

            migrationBuilder.DropColumn(
                name: "AccountHolderName",
                table: "Bank");

            migrationBuilder.DropColumn(
                name: "DateOfBirth",
                table: "Bank");

            migrationBuilder.RenameColumn(
                name: "Gender",
                table: "Bank",
                newName: "Name");

            migrationBuilder.AddColumn<int>(
                name: "Balance",
                table: "Bank",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
