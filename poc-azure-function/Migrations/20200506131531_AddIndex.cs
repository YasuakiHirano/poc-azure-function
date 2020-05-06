using Microsoft.EntityFrameworkCore.Migrations;

namespace poc_azure_function.Migrations
{
    public partial class AddIndex : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex("idx_messages_on_board_id", "messages", "board_id");
            // migrationBuilder.CreateIndex("idx_users_on_name_email", "users", new string[] { "name", "email" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex("idx_messages_on_board_id");
            // migrationBuilder.DropIndex("idx_users_on_name_email");
        }
    }
}
