using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Backend.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RenameResourceToActivity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_assessments_course_resources_resource_id",
                table: "assessments");

            migrationBuilder.DropForeignKey(
                name: "fk_assignments_course_resources_resource_id",
                table: "assignments");

            migrationBuilder.DropForeignKey(
                name: "fk_courses_users_creator_id",
                table: "courses");

            migrationBuilder.DropForeignKey(
                name: "fk_lessons_course_resources_resource_id",
                table: "lessons");

            migrationBuilder.DropForeignKey(
                name: "fk_problems_course_resources_resource_id",
                table: "problems");

            migrationBuilder.DropForeignKey(
                name: "fk_resource_comments_course_resources_activity_id",
                table: "resource_comments");

            migrationBuilder.DropForeignKey(
                name: "fk_resource_comments_resource_comments_parent_comment_id",
                table: "resource_comments");

            migrationBuilder.DropForeignKey(
                name: "fk_resource_comments_users_user_id",
                table: "resource_comments");

            migrationBuilder.DropTable(
                name: "resource_progress");

            migrationBuilder.DropTable(
                name: "course_resources");

            migrationBuilder.DropPrimaryKey(
                name: "pk_resource_comments",
                table: "resource_comments");

            migrationBuilder.DropColumn(
                name: "code",
                table: "courses");

            migrationBuilder.RenameTable(
                name: "resource_comments",
                newName: "activity_comments");

            migrationBuilder.RenameColumn(
                name: "resource_id",
                table: "problems",
                newName: "activity_id");

            migrationBuilder.RenameIndex(
                name: "ix_problems_resource_id",
                table: "problems",
                newName: "ix_problems_activity_id");

            migrationBuilder.RenameColumn(
                name: "resource_id",
                table: "lessons",
                newName: "activity_id");

            migrationBuilder.RenameIndex(
                name: "ix_lessons_resource_id",
                table: "lessons",
                newName: "ix_lessons_activity_id");

            migrationBuilder.RenameColumn(
                name: "creator_id",
                table: "courses",
                newName: "instructor_id");

            migrationBuilder.RenameIndex(
                name: "ix_courses_creator_id_title",
                table: "courses",
                newName: "ix_courses_instructor_id_title");

            migrationBuilder.RenameColumn(
                name: "resource_id",
                table: "assignments",
                newName: "activity_id");

            migrationBuilder.RenameIndex(
                name: "ix_assignments_resource_id",
                table: "assignments",
                newName: "ix_assignments_activity_id");

            migrationBuilder.RenameColumn(
                name: "resource_id",
                table: "assessments",
                newName: "activity_id");

            migrationBuilder.RenameIndex(
                name: "ix_assessments_resource_id",
                table: "assessments",
                newName: "ix_assessments_activity_id");

            migrationBuilder.RenameIndex(
                name: "ix_resource_comments_user_id",
                table: "activity_comments",
                newName: "ix_activity_comments_user_id");

            migrationBuilder.RenameIndex(
                name: "ix_resource_comments_parent_comment_id",
                table: "activity_comments",
                newName: "ix_activity_comments_parent_comment_id");

            migrationBuilder.RenameIndex(
                name: "ix_resource_comments_activity_id",
                table: "activity_comments",
                newName: "ix_activity_comments_activity_id");

            migrationBuilder.AlterColumn<string>(
                name: "description",
                table: "courses",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "description",
                table: "assessments",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "pk_activity_comments",
                table: "activity_comments",
                column: "id");

            migrationBuilder.CreateTable(
                name: "course_activities",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    module_id = table.Column<long>(type: "bigint", nullable: false),
                    type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    order_index = table.Column<int>(type: "integer", nullable: false),
                    is_published = table.Column<bool>(type: "boolean", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_course_activities", x => x.id);
                    table.ForeignKey(
                        name: "fk_course_activities_course_modules_module_id",
                        column: x => x.module_id,
                        principalTable: "modules",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "activity_progress",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<long>(type: "bigint", nullable: false),
                    activity_id = table.Column<long>(type: "bigint", nullable: false),
                    is_completed = table.Column<bool>(type: "boolean", nullable: false),
                    completed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    last_accessed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_activity_progress", x => x.id);
                    table.ForeignKey(
                        name: "fk_activity_progress_course_activities_activity_id",
                        column: x => x.activity_id,
                        principalTable: "course_activities",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_activity_progress_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_activity_progress_activity_id",
                table: "activity_progress",
                column: "activity_id");

            migrationBuilder.CreateIndex(
                name: "ix_activity_progress_user_id_activity_id",
                table: "activity_progress",
                columns: new[] { "user_id", "activity_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_course_activities_module_id",
                table: "course_activities",
                column: "module_id");

            migrationBuilder.AddForeignKey(
                name: "fk_activity_comments_activity_comments_parent_comment_id",
                table: "activity_comments",
                column: "parent_comment_id",
                principalTable: "activity_comments",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_activity_comments_course_activities_activity_id",
                table: "activity_comments",
                column: "activity_id",
                principalTable: "course_activities",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_activity_comments_users_user_id",
                table: "activity_comments",
                column: "user_id",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_assessments_course_activities_activity_id",
                table: "assessments",
                column: "activity_id",
                principalTable: "course_activities",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_assignments_course_activities_activity_id",
                table: "assignments",
                column: "activity_id",
                principalTable: "course_activities",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_courses_users_instructor_id",
                table: "courses",
                column: "instructor_id",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_lessons_course_activities_activity_id",
                table: "lessons",
                column: "activity_id",
                principalTable: "course_activities",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_problems_course_activities_activity_id",
                table: "problems",
                column: "activity_id",
                principalTable: "course_activities",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_activity_comments_activity_comments_parent_comment_id",
                table: "activity_comments");

            migrationBuilder.DropForeignKey(
                name: "fk_activity_comments_course_activities_activity_id",
                table: "activity_comments");

            migrationBuilder.DropForeignKey(
                name: "fk_activity_comments_users_user_id",
                table: "activity_comments");

            migrationBuilder.DropForeignKey(
                name: "fk_assessments_course_activities_activity_id",
                table: "assessments");

            migrationBuilder.DropForeignKey(
                name: "fk_assignments_course_activities_activity_id",
                table: "assignments");

            migrationBuilder.DropForeignKey(
                name: "fk_courses_users_instructor_id",
                table: "courses");

            migrationBuilder.DropForeignKey(
                name: "fk_lessons_course_activities_activity_id",
                table: "lessons");

            migrationBuilder.DropForeignKey(
                name: "fk_problems_course_activities_activity_id",
                table: "problems");

            migrationBuilder.DropTable(
                name: "activity_progress");

            migrationBuilder.DropTable(
                name: "course_activities");

            migrationBuilder.DropPrimaryKey(
                name: "pk_activity_comments",
                table: "activity_comments");

            migrationBuilder.RenameTable(
                name: "activity_comments",
                newName: "resource_comments");

            migrationBuilder.RenameColumn(
                name: "activity_id",
                table: "problems",
                newName: "resource_id");

            migrationBuilder.RenameIndex(
                name: "ix_problems_activity_id",
                table: "problems",
                newName: "ix_problems_resource_id");

            migrationBuilder.RenameColumn(
                name: "activity_id",
                table: "lessons",
                newName: "resource_id");

            migrationBuilder.RenameIndex(
                name: "ix_lessons_activity_id",
                table: "lessons",
                newName: "ix_lessons_resource_id");

            migrationBuilder.RenameColumn(
                name: "instructor_id",
                table: "courses",
                newName: "creator_id");

            migrationBuilder.RenameIndex(
                name: "ix_courses_instructor_id_title",
                table: "courses",
                newName: "ix_courses_creator_id_title");

            migrationBuilder.RenameColumn(
                name: "activity_id",
                table: "assignments",
                newName: "resource_id");

            migrationBuilder.RenameIndex(
                name: "ix_assignments_activity_id",
                table: "assignments",
                newName: "ix_assignments_resource_id");

            migrationBuilder.RenameColumn(
                name: "activity_id",
                table: "assessments",
                newName: "resource_id");

            migrationBuilder.RenameIndex(
                name: "ix_assessments_activity_id",
                table: "assessments",
                newName: "ix_assessments_resource_id");

            migrationBuilder.RenameIndex(
                name: "ix_activity_comments_user_id",
                table: "resource_comments",
                newName: "ix_resource_comments_user_id");

            migrationBuilder.RenameIndex(
                name: "ix_activity_comments_parent_comment_id",
                table: "resource_comments",
                newName: "ix_resource_comments_parent_comment_id");

            migrationBuilder.RenameIndex(
                name: "ix_activity_comments_activity_id",
                table: "resource_comments",
                newName: "ix_resource_comments_activity_id");

            migrationBuilder.AlterColumn<string>(
                name: "description",
                table: "courses",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<string>(
                name: "code",
                table: "courses",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "description",
                table: "assessments",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddPrimaryKey(
                name: "pk_resource_comments",
                table: "resource_comments",
                column: "id");

            migrationBuilder.CreateTable(
                name: "course_resources",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    module_id = table.Column<long>(type: "bigint", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    is_published = table.Column<bool>(type: "boolean", nullable: false),
                    order_index = table.Column<int>(type: "integer", nullable: false),
                    title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_course_resources", x => x.id);
                    table.ForeignKey(
                        name: "fk_course_resources_course_modules_module_id",
                        column: x => x.module_id,
                        principalTable: "modules",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "resource_progress",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    resource_id = table.Column<long>(type: "bigint", nullable: false),
                    user_id = table.Column<long>(type: "bigint", nullable: false),
                    completed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    is_completed = table.Column<bool>(type: "boolean", nullable: false),
                    last_accessed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_resource_progress", x => x.id);
                    table.ForeignKey(
                        name: "fk_resource_progress_course_resources_resource_id",
                        column: x => x.resource_id,
                        principalTable: "course_resources",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_resource_progress_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_course_resources_module_id",
                table: "course_resources",
                column: "module_id");

            migrationBuilder.CreateIndex(
                name: "ix_resource_progress_resource_id",
                table: "resource_progress",
                column: "resource_id");

            migrationBuilder.CreateIndex(
                name: "ix_resource_progress_user_id_resource_id",
                table: "resource_progress",
                columns: new[] { "user_id", "resource_id" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "fk_assessments_course_resources_resource_id",
                table: "assessments",
                column: "resource_id",
                principalTable: "course_resources",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_assignments_course_resources_resource_id",
                table: "assignments",
                column: "resource_id",
                principalTable: "course_resources",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_courses_users_creator_id",
                table: "courses",
                column: "creator_id",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_lessons_course_resources_resource_id",
                table: "lessons",
                column: "resource_id",
                principalTable: "course_resources",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_problems_course_resources_resource_id",
                table: "problems",
                column: "resource_id",
                principalTable: "course_resources",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_resource_comments_course_resources_activity_id",
                table: "resource_comments",
                column: "activity_id",
                principalTable: "course_resources",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_resource_comments_resource_comments_parent_comment_id",
                table: "resource_comments",
                column: "parent_comment_id",
                principalTable: "resource_comments",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_resource_comments_users_user_id",
                table: "resource_comments",
                column: "user_id",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
