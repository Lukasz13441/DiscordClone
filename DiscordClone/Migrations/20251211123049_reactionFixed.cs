using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DiscordClone.Migrations
{
    /// <inheritdoc />
    public partial class reactionFixed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Count",
                table: "MessageReactions",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Count",
                table: "MessageReactions");
        }
    }
}
