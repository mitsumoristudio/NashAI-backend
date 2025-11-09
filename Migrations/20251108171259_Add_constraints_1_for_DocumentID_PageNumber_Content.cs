using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NashAI_app.Migrations
{
    /// <inheritdoc />
    public partial class Add_constraints_1_for_DocumentID_PageNumber_Content : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameIndex(
                name: "constraint_1",
                table: "document_embedding",
                newName: "constraint_1");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameIndex(
                name: "constraint_1",
                table: "document_embedding",
                newName: "UQ_Document_Page_Content");
        }
    }
}
