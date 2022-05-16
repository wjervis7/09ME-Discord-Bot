using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace _09.Mass.Extinction.Web.Data.Migrations
{
    public partial class UpdateUserIdToUlong : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "DiscordUserIdTemp",
                table: "DiscordUsers",
                type: "decimal(20,0)",
                nullable: true);

            migrationBuilder.Sql(@"
UPDATE DiscordUsers
SET DiscordUserIdTemp = CAST(DiscordUserId AS DECIMAL(20,0));
");

            migrationBuilder.Sql(@"
ALTER TABLE DiscordUsers
DROP CONSTRAINT PK_DiscordUsers;
");

            migrationBuilder.AlterColumn<decimal>(
                name: "DiscordUserId",
                table: "DiscordUsers",
                type: "decimal(20,0)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.Sql(@"
UPDATE DiscordUsers
SET DiscordUserId = DiscordUserIdTemp;
");
            
            migrationBuilder.Sql(@"
ALTER TABLE DiscordUsers
ADD CONSTRAINT PK_DiscordUsers PRIMARY KEY CLUSTERED (DiscordUserId);
");

            migrationBuilder.DropColumn(
                name: "DiscordUserIdTemp",
                table: "DiscordUsers");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "AspNetUserTokens",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(128)",
                oldMaxLength: 128);

            migrationBuilder.AlterColumn<string>(
                name: "LoginProvider",
                table: "AspNetUserTokens",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(128)",
                oldMaxLength: 128);

            migrationBuilder.AlterColumn<string>(
                name: "ProfilePicture",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ProviderKey",
                table: "AspNetUserLogins",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(128)",
                oldMaxLength: 128);

            migrationBuilder.AlterColumn<string>(
                name: "LoginProvider",
                table: "AspNetUserLogins",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(128)",
                oldMaxLength: 128);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "DiscordUserIdTemp",
                table: "DiscordUsers",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.Sql(@"
UPDATE DiscordUsers
SET DiscordUserIdTemp = CAST(DiscordUserId AS nvarchar(450));
");

            migrationBuilder.Sql(@"
ALTER TABLE DiscordUsers
DROP CONSTRAINT PK_DiscordUsers;
");

            migrationBuilder.AlterColumn<string>(
                name: "DiscordUserId",
                table: "DiscordUsers",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(20,0)");

            migrationBuilder.Sql(@"
UPDATE DiscordUsers
SET DiscordUserId = DiscordUserIdTemp;
");
            
            migrationBuilder.Sql(@"
ALTER TABLE DiscordUsers
ADD CONSTRAINT PK_DiscordUsers PRIMARY KEY CLUSTERED (DiscordUserId);
");

            migrationBuilder.DropColumn(
                name: "DiscordUserIdTemp",
                table: "DiscordUsers");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "AspNetUserTokens",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "LoginProvider",
                table: "AspNetUserTokens",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "ProfilePicture",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "ProviderKey",
                table: "AspNetUserLogins",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "LoginProvider",
                table: "AspNetUserLogins",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");
        }
    }
}
