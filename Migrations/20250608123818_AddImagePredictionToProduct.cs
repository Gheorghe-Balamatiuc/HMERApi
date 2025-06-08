using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HMERApi.Migrations
{
    /// <inheritdoc />
    public partial class AddImagePredictionToProduct : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ImagePrediction",
                table: "Products",
                type: "nvarchar(max)",
                maxLength: 8000,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImagePrediction",
                table: "Products");
        }
    }
}
