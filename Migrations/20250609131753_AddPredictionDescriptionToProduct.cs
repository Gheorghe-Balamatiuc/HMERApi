using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HMERApi.Migrations
{
    /// <inheritdoc />
    public partial class AddPredictionDescriptionToProduct : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PredictionDescription",
                table: "Products",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PredictionDescription",
                table: "Products");
        }
    }
}
