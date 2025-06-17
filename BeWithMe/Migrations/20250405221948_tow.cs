using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BeWithMe.Migrations
{
    /// <inheritdoc />
    public partial class tow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_Profiles_HelperProfileId",
                table: "AspNetUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_Profiles_PatientProfileId",
                table: "AspNetUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_Helpers_AspNetUsers_UserId",
                table: "Helpers");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_HelperProfileId",
                table: "AspNetUsers");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_PatientProfileId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "HelperProfileId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "PatientProfileId",
                table: "AspNetUsers");

            migrationBuilder.AddColumn<int>(
                name: "ProfileId",
                table: "Patients",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Patients_ProfileId",
                table: "Patients",
                column: "ProfileId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Helpers_AspNetUsers_UserId",
                table: "Helpers",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);

            migrationBuilder.AddForeignKey(
                name: "FK_Patients_Profiles_ProfileId",
                table: "Patients",
                column: "ProfileId",
                principalTable: "Profiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Helpers_AspNetUsers_UserId",
                table: "Helpers");

            migrationBuilder.DropForeignKey(
                name: "FK_Patients_Profiles_ProfileId",
                table: "Patients");

            migrationBuilder.DropIndex(
                name: "IX_Patients_ProfileId",
                table: "Patients");

            migrationBuilder.DropColumn(
                name: "ProfileId",
                table: "Patients");

            migrationBuilder.AddColumn<int>(
                name: "HelperProfileId",
                table: "AspNetUsers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "PatientProfileId",
                table: "AspNetUsers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_HelperProfileId",
                table: "AspNetUsers",
                column: "HelperProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_PatientProfileId",
                table: "AspNetUsers",
                column: "PatientProfileId");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_Profiles_HelperProfileId",
                table: "AspNetUsers",
                column: "HelperProfileId",
                principalTable: "Profiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_Profiles_PatientProfileId",
                table: "AspNetUsers",
                column: "PatientProfileId",
                principalTable: "Profiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Helpers_AspNetUsers_UserId",
                table: "Helpers",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }
    }
}
