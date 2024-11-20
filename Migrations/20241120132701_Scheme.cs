using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GovConnect.Migrations
{
    /// <inheritdoc />
    public partial class Scheme : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.CreateTable(
                name: "GovSchemes",
                columns: table => new
                {
                    SchemeID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SchemeName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Details = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Attributes = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Eligibility = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ApplicationProcess = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DocsRequired = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GovSchemes", x => x.SchemeID);
                });

            migrationBuilder.CreateTable(
                name: "SchemeEligibilities",
                columns: table => new
                {
                    EligibilityId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SchemeId = table.Column<int>(type: "int", nullable: false),
                    Gender = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Age = table.Column<int>(type: "int", nullable: false),
                    MinAge = table.Column<int>(type: "int", nullable: false),
                    MaxAge = table.Column<int>(type: "int", nullable: false),
                    Caste = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsDifferentlyAbled = table.Column<bool>(type: "bit", nullable: true),
                    IsStudent = table.Column<bool>(type: "bit", nullable: true),
                    IsBPL = table.Column<bool>(type: "bit", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SchemeEligibilities", x => x.EligibilityId);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GovSchemes");

            migrationBuilder.DropTable(
                name: "SchemeEligibilities");

        }
    }
}
