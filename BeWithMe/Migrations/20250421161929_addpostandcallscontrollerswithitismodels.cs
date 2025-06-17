using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BeWithMe.Migrations
{
    /// <inheritdoc />
    public partial class addpostandcallscontrollerswithitismodels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HelpSessions");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
