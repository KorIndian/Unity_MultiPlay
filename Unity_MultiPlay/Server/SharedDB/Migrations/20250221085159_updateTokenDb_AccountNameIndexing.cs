using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SharedDB.Migrations
{
    /// <inheritdoc />
    public partial class updateTokenDb_AccountNameIndexing : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "AccountName",
                table: "Token",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "IX_Token_AccountName",
                table: "Token",
                column: "AccountName",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Token_AccountName",
                table: "Token");

            migrationBuilder.AlterColumn<string>(
                name: "AccountName",
                table: "Token",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");
        }
    }
}
