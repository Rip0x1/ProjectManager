using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using ProjectManagementSystem.Database.Data;

#nullable disable

namespace ProjectManagementSystem.Database.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20261008153000_RenameEmailToLogin")]
    public partial class RenameEmailToLogin : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
UPDATE Users SET Email = CASE WHEN CHARINDEX('@', Email) > 1 THEN LEFT(Email, CHARINDEX('@', Email) - 1) ELSE Email END;
UPDATE Users SET Email = N'admin' WHERE LOWER(Email) IN (N'admin', N'admin@admin.com');
;WITH d AS (
    SELECT Id, ROW_NUMBER() OVER (PARTITION BY Email ORDER BY Id) AS rn
    FROM Users
)
UPDATE u SET Email = u.Email + CAST(u.Id AS nvarchar(20))
FROM Users u
INNER JOIN d ON d.Id = u.Id
WHERE d.rn > 1 AND LOWER(u.Email) <> N'admin';
UPDATE AuditLogs SET UserEmail = CASE WHEN CHARINDEX('@', UserEmail) > 1 THEN LEFT(UserEmail, CHARINDEX('@', UserEmail) - 1) ELSE UserEmail END;
");
            migrationBuilder.DropIndex(
                name: "IX_Users_Email",
                table: "Users");
            migrationBuilder.RenameColumn(
                name: "Email",
                table: "Users",
                newName: "Login");
            migrationBuilder.AlterColumn<string>(
                name: "Login",
                table: "Users",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(255)",
                oldMaxLength: 255);
            migrationBuilder.CreateIndex(
                name: "IX_Users_Login",
                table: "Users",
                column: "Login",
                unique: true);
            migrationBuilder.RenameColumn(
                name: "UserEmail",
                table: "AuditLogs",
                newName: "UserLogin");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Users_Login",
                table: "Users");
            migrationBuilder.RenameColumn(
                name: "Login",
                table: "Users",
                newName: "Email");
            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "Users",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);
            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);
            migrationBuilder.RenameColumn(
                name: "UserLogin",
                table: "AuditLogs",
                newName: "UserEmail");
        }
    }
}
