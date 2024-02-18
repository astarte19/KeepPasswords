using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KeepPasswords.Migrations
{
    public partial class Calendar2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Color",
                table: "UserCalendarEvents",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Color",
                table: "UserCalendarEvents");
        }
    }
}
