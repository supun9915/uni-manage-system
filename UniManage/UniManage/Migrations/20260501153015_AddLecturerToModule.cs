using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UniManage.Migrations
{
    /// <inheritdoc />
    public partial class AddLecturerToModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "LecturerId",
                table: "Modules",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Modules_LecturerId",
                table: "Modules",
                column: "LecturerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Modules_Users_LecturerId",
                table: "Modules",
                column: "LecturerId",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Modules_Users_LecturerId",
                table: "Modules");

            migrationBuilder.DropIndex(
                name: "IX_Modules_LecturerId",
                table: "Modules");

            migrationBuilder.DropColumn(
                name: "LecturerId",
                table: "Modules");
        }
    }
}
