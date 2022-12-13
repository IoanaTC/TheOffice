using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TheOffice.Data.Migrations
{
    public partial class Migratie2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Task_AspNetUsers_UserId1",
                table: "Task");

            migrationBuilder.DropIndex(
                name: "IX_Task_UserId1",
                table: "Task");

            migrationBuilder.DropColumn(
                name: "UserId1",
                table: "Task");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "Task",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Task_UserId",
                table: "Task",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Task_AspNetUsers_UserId",
                table: "Task",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Task_AspNetUsers_UserId",
                table: "Task");

            migrationBuilder.DropIndex(
                name: "IX_Task_UserId",
                table: "Task");

            migrationBuilder.AlterColumn<int>(
                name: "UserId",
                table: "Task",
                type: "int",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UserId1",
                table: "Task",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Task_UserId1",
                table: "Task",
                column: "UserId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Task_AspNetUsers_UserId1",
                table: "Task",
                column: "UserId1",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }
    }
}
