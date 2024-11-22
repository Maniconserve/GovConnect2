using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GovConnect.Migrations
{
    /// <inheritdoc />
    public partial class ServiceNameColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ServiceName",
                table: "ServiceApplications",
                type: "nvarchar(max)",
                nullable: true,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ServiceName",
                table: "ServiceApplications");
        }
    }
}
