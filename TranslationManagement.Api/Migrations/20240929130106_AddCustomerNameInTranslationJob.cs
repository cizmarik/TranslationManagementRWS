using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TranslationManagement.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddCustomerNameInTranslationJob : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TranslatorID",
                table: "TranslationJobs",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TranslatorID",
                table: "TranslationJobs");
        }
    }
}
