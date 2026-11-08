using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UserRemoveUsernameFullName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_users_username",
                table: "users");

            migrationBuilder.DropColumn(
                name: "avatar_url",
                table: "users");

            migrationBuilder.DropColumn(
                name: "username",
                table: "users");

            migrationBuilder.RenameColumn(
                name: "fullname",
                table: "users",
                newName: "last_name");

            migrationBuilder.AddColumn<long>(
                name: "avatar_file_id",
                table: "users",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "first_name",
                table: "users",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<long>(
                name: "course_id1",
                table: "recent_courses",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "user_id1",
                table: "recent_courses",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_recent_courses_course_id1",
                table: "recent_courses",
                column: "course_id1");

            migrationBuilder.CreateIndex(
                name: "ix_recent_courses_user_id1",
                table: "recent_courses",
                column: "user_id1");

            migrationBuilder.AddForeignKey(
                name: "fk_recent_courses_courses_course_id1",
                table: "recent_courses",
                column: "course_id1",
                principalTable: "courses",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_recent_courses_users_user_id1",
                table: "recent_courses",
                column: "user_id1",
                principalTable: "users",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_recent_courses_courses_course_id1",
                table: "recent_courses");

            migrationBuilder.DropForeignKey(
                name: "fk_recent_courses_users_user_id1",
                table: "recent_courses");

            migrationBuilder.DropIndex(
                name: "ix_recent_courses_course_id1",
                table: "recent_courses");

            migrationBuilder.DropIndex(
                name: "ix_recent_courses_user_id1",
                table: "recent_courses");

            migrationBuilder.DropColumn(
                name: "avatar_file_id",
                table: "users");

            migrationBuilder.DropColumn(
                name: "first_name",
                table: "users");

            migrationBuilder.DropColumn(
                name: "course_id1",
                table: "recent_courses");

            migrationBuilder.DropColumn(
                name: "user_id1",
                table: "recent_courses");

            migrationBuilder.RenameColumn(
                name: "last_name",
                table: "users",
                newName: "fullname");

            migrationBuilder.AddColumn<string>(
                name: "avatar_url",
                table: "users",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "username",
                table: "users",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "ix_users_username",
                table: "users",
                column: "username",
                unique: true);
        }
    }
}
