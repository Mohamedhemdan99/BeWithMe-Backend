using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BeWithMe.Migrations
{
    /// <inheritdoc />
    public partial class modfycallhistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DisconnectReason",
                table: "CallHistories",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "LastHeartbeat",
                table: "CallHistories",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "CallHistories",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DisconnectReason",
                table: "CallHistories");

            migrationBuilder.DropColumn(
                name: "LastHeartbeat",
                table: "CallHistories");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "CallHistories");
        }
    }
}
