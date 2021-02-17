using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace AspNetCore.Data.Migrations
{
    public partial class initialsetup : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TestMembers",
                columns: table => new
                {
                    OID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    x = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    y = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    case_code = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    confirmation_date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    municipality_code = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    municipality_name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    age_bracket = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    gender = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TestMembers", x => x.OID);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TestMembers");
        }
    }
}
