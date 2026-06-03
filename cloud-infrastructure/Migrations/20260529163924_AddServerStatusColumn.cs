using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace cloud_infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddServerStatusColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "tbl_ProvisionedServers",
                type: "TEXT",
                nullable: false,
                defaultValue: "Pending");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                table: "tbl_ProvisionedServers");
        }
    }
}
