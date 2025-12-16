using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DiscordClone.Migrations
{
    /// <inheritdoc />
    public partial class ChannelRoom : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Friendship_UserProfile_FriendId",
                table: "Friendship");

            migrationBuilder.DropForeignKey(
                name: "FK_Server_UserProfile_OwnerId",
                table: "Server");

            migrationBuilder.DropForeignKey(
                name: "FK_UserProfile_AspNetUsers_UserId",
                table: "UserProfile");

            migrationBuilder.DropForeignKey(
                name: "FK_VoiceChannel_Server_VoiceChannelId",
                table: "VoiceChannel");

            migrationBuilder.DropIndex(
                name: "IX_VoiceChannel_VoiceChannelId",
                table: "VoiceChannel");


            migrationBuilder.AddColumn<int>(
                name: "RoomId",
                table: "UserProfile",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsBanned",
                table: "ServerMember",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "Range",
                table: "ServerMember",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<int>(
                name: "Count",
                table: "MessageReactions",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.CreateTable(
                name: "ChannelRoom",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ChannelId = table.Column<int>(type: "int", nullable: false),
                    userId = table.Column<int>(type: "int", nullable: false),
                    ConnectionId = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChannelRoom", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChannelRoom_Channel_ChannelId",
                        column: x => x.ChannelId,
                        principalTable: "Channel",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ChannelRoom_UserProfile_userId",
                        column: x => x.userId,
                        principalTable: "UserProfile",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Friendship_UserId",
                table: "Friendship",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ChannelRoom_ChannelId",
                table: "ChannelRoom",
                column: "ChannelId");

            migrationBuilder.CreateIndex(
                name: "IX_ChannelRoom_userId",
                table: "ChannelRoom",
                column: "userId");

            migrationBuilder.AddForeignKey(
                name: "FK_Friendship_UserProfile_FriendId",
                table: "Friendship",
                column: "FriendId",
                principalTable: "UserProfile",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Friendship_UserProfile_UserId",
                table: "Friendship",
                column: "UserId",
                principalTable: "UserProfile",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Server_UserProfile_OwnerId",
                table: "Server",
                column: "OwnerId",
                principalTable: "UserProfile",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_UserProfile_AspNetUsers_UserId",
                table: "UserProfile",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_VoiceChannel_Server_Id",
                table: "VoiceChannel",
                column: "Id",
                principalTable: "Server",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Friendship_UserProfile_FriendId",
                table: "Friendship");

            migrationBuilder.DropForeignKey(
                name: "FK_Friendship_UserProfile_UserId",
                table: "Friendship");

            migrationBuilder.DropForeignKey(
                name: "FK_Server_UserProfile_OwnerId",
                table: "Server");

            migrationBuilder.DropForeignKey(
                name: "FK_UserProfile_AspNetUsers_UserId",
                table: "UserProfile");

            migrationBuilder.DropForeignKey(
                name: "FK_VoiceChannel_Server_Id",
                table: "VoiceChannel");

            migrationBuilder.DropTable(
                name: "ChannelRoom");

            migrationBuilder.DropIndex(
                name: "IX_Friendship_UserId",
                table: "Friendship");

            migrationBuilder.DropColumn(
                name: "RoomId",
                table: "UserProfile");

            migrationBuilder.DropColumn(
                name: "IsBanned",
                table: "ServerMember");

            migrationBuilder.DropColumn(
                name: "Range",
                table: "ServerMember");

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "VoiceChannel",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AlterColumn<int>(
                name: "Count",
                table: "MessageReactions",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_VoiceChannel_VoiceChannelId",
                table: "VoiceChannel",
                column: "VoiceChannelId");

            migrationBuilder.AddForeignKey(
                name: "FK_Friendship_UserProfile_FriendId",
                table: "Friendship",
                column: "FriendId",
                principalTable: "UserProfile",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Server_UserProfile_OwnerId",
                table: "Server",
                column: "OwnerId",
                principalTable: "UserProfile",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserProfile_AspNetUsers_UserId",
                table: "UserProfile",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_VoiceChannel_Server_VoiceChannelId",
                table: "VoiceChannel",
                column: "VoiceChannelId",
                principalTable: "Server",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
