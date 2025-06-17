using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BeWithMe.Migrations
{
    /// <inheritdoc />
    public partial class modifyNotificaitonRetreivalandcreation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RecipientId",
                table: "Notifications",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RecipientId",
                table: "Notifications");
        }
    }
}
