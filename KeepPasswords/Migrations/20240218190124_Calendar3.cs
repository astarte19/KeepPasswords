using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KeepPasswords.Migrations
{
    public partial class Calendar3 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "UserCalendarEvents",
                type: "TEXT",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "UserCalendarEvents");
        }
    }
}
