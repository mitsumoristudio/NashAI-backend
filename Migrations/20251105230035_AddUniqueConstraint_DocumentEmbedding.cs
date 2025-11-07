using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NashAI_app.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueConstraint_DocumentEmbedding : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameIndex(
                name: "UQ_Document_Page_Content",
                table: "document_embedding",
                newName: "UQ_Document_Page_Content");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameIndex(
                name: "UQ_Document_Page_Content",
                table: "document_embedding",
                newName: "IX_document_embedding_DocumentId_PageNumber_Content");
        }
    }
}
