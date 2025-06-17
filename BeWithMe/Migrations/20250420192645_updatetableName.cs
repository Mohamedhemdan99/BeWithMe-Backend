using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BeWithMe.Migrations
{
    /// <inheritdoc />
    public partial class updatetableName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            

            migrationBuilder.CreateTable(
                name: "hubGroups",
                columns: table => new
                {
                    GroupId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    GroupName = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_hubGroups", x => x.GroupId);
                });

            migrationBuilder.CreateTable(
                name: "hubUserConnections",
                columns: table => new
                {
                    ConnectionId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_hubUserConnections", x => x.ConnectionId);
                });

            migrationBuilder.CreateTable(
                name: "hubUserGroups",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    GroupId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_hubUserGroups", x => new { x.UserId, x.GroupId });
                });

            migrationBuilder.CreateIndex(
                name: "IX_hubGroups_GroupName",
                table: "hubGroups",
                column: "GroupName",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "hubGroups");

            migrationBuilder.DropTable(
                name: "hubUserConnections");

            migrationBuilder.DropTable(
                name: "hubUserGroups");

            migrationBuilder.RenameColumn(
                name: "Content",
                table: "Notifications",
                newName: "Message");
        }
    }
}
