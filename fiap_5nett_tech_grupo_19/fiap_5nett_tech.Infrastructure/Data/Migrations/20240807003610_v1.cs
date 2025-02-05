using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace fiap_5nett_tech.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class v1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Region",
                columns: table => new
                {
                    Ddd = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "NVARCHAR(80)", maxLength: 80, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Region", x => x.Ddd);
                });

            migrationBuilder.CreateTable(
                name: "Contact",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "NVARCHAR(80)", maxLength: 80, nullable: false),
                    Email = table.Column<string>(type: "NVARCHAR(255)", maxLength: 255, nullable: false),
                    Phone = table.Column<string>(type: "VARCHAR(9)", maxLength: 9, nullable: false),
                    Ddd = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Contact", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CONTACT_REGION",
                        column: x => x.Ddd,
                        principalTable: "Region",
                        principalColumn: "Ddd",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Region",
                columns: new[] { "Ddd", "Name" },
                values: new object[,]
                {
                    { 11, "São Paulo" },
                    { 12, "São Paulo" },
                    { 13, "São Paulo" },
                    { 14, "São Paulo" },
                    { 15, "São Paulo" },
                    { 16, "São Paulo" },
                    { 17, "São Paulo" },
                    { 18, "São Paulo" },
                    { 19, "São Paulo" },
                    { 21, "Rio de Janeiro" },
                    { 22, "Rio de Janeiro" },
                    { 24, "Rio de Janeiro" },
                    { 27, "Espírito Santo" },
                    { 28, "Espírito Santo" },
                    { 31, "Minas Gerais" },
                    { 32, "Minas Gerais" },
                    { 33, "Minas Gerais" },
                    { 34, "Minas Gerais" },
                    { 35, "Minas Gerais" },
                    { 37, "Minas Gerais" },
                    { 38, "Minas Gerais" },
                    { 41, "Paraná" },
                    { 42, "Paraná" },
                    { 43, "Paraná" },
                    { 44, "Paraná" },
                    { 45, "Paraná" },
                    { 46, "Paraná" },
                    { 47, "Santa Catarina" },
                    { 48, "Santa Catarina" },
                    { 49, "Santa Catarina" },
                    { 51, "Rio Grande do Sul" },
                    { 53, "Rio Grande do Sul" },
                    { 54, "Rio Grande do Sul" },
                    { 55, "Rio Grande do Sul" },
                    { 61, "Distrito Federal" },
                    { 62, "Goiás" },
                    { 63, "Tocantins" },
                    { 64, "Goiás" },
                    { 65, "Mato Grosso" },
                    { 66, "Mato Grosso" },
                    { 67, "Mato Grosso do Sul" },
                    { 68, "Acre" },
                    { 69, "Rondônia" },
                    { 71, "Bahia" },
                    { 73, "Bahia" },
                    { 74, "Bahia" },
                    { 75, "Bahia" },
                    { 77, "Bahia" },
                    { 79, "Sergipe" },
                    { 81, "Pernambuco" },
                    { 82, "Alagoas" },
                    { 83, "Paraíba" },
                    { 84, "Rio Grande do Norte" },
                    { 85, "Ceará" },
                    { 86, "Piauí" },
                    { 87, "Pernambuco" },
                    { 88, "Ceará" },
                    { 89, "Piauí" },
                    { 91, "Pará" },
                    { 92, "Amazonas" },
                    { 93, "Pará" },
                    { 94, "Pará" },
                    { 95, "Roraima" },
                    { 96, "Amapá" },
                    { 97, "Amazonas" },
                    { 98, "Maranhão" },
                    { 99, "Maranhão" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Contact_Ddd",
                table: "Contact",
                column: "Ddd");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Contact");

            migrationBuilder.DropTable(
                name: "Region");
        }
    }
}
