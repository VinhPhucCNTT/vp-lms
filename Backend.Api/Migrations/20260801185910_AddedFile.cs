using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class AddedFile : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_assessments_module_resources_resource_id",
                table: "assessments");

            migrationBuilder.DropForeignKey(
                name: "fk_assignments_module_resources_resource_id",
                table: "assignments");

            migrationBuilder.DropForeignKey(
                name: "fk_lessons_module_resources_resource_id",
                table: "lessons");

            migrationBuilder.DropForeignKey(
                name: "fk_problems_module_resources_resource_id",
                table: "problems");

            migrationBuilder.DropForeignKey(
                name: "fk_resource_comments_module_resources_resource_id",
                table: "resource_comments");

            migrationBuilder.DropForeignKey(
                name: "fk_resource_progress_module_resources_resource_id",
                table: "resource_progress");

            migrationBuilder.DropTable(
                name: "module_resources");

            migrationBuilder.DropIndex(
                name: "ix_resource_progress_user_id_resource_id",
                table: "resource_progress");

            migrationBuilder.DropIndex(
                name: "ix_problem_test_cases_problem_id_order_index",
                table: "problem_test_cases");

            migrationBuilder.DropIndex(
                name: "ix_assignment_submissions_assignment_id_user_id",
                table: "assignment_submissions");

            migrationBuilder.DropIndex(
                name: "ix_assessment_attempts_assessment_id_user_id_attempt_number",
                table: "assessment_attempts");

            migrationBuilder.DropColumn(
                name: "explanation",
                table: "problem_test_cases");

            migrationBuilder.DropColumn(
                name: "thumbnail_url",
                table: "courses");

            migrationBuilder.RenameColumn(
                name: "resource_id",
                table: "resource_comments",
                newName: "activity_id");

            migrationBuilder.RenameIndex(
                name: "ix_resource_comments_resource_id",
                table: "resource_comments",
                newName: "ix_resource_comments_activity_id");

            migrationBuilder.RenameColumn(
                name: "memory_limit_mb",
                table: "problems",
                newName: "memory_limit_kb");

            migrationBuilder.RenameColumn(
                name: "instructions_markdown",
                table: "assignments",
                newName: "instructions_md");

            migrationBuilder.AddColumn<int>(
                name: "difficulty",
                table: "problems",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "order_index",
                table: "problem_submissions",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<long>(
                name: "background_file_id",
                table: "courses",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "code",
                table: "courses",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<long>(
                name: "department_id",
                table: "courses",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "section_id",
                table: "assessment_questions",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.CreateTable(
                name: "course_resources",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    module_id = table.Column<long>(type: "bigint", nullable: false),
                    type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    order_index = table.Column<int>(type: "integer", nullable: false),
                    is_published = table.Column<bool>(type: "boolean", nullable: false),
                    available_from = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    available_until = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    access_password = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
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
                name: "departments",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_departments", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "file_assets",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    uploader_id = table.Column<long>(type: "bigint", nullable: false),
                    original_file_name = table.Column<string>(type: "text", nullable: false),
                    storage_path = table.Column<string>(type: "text", nullable: false),
                    content_type = table.Column<string>(type: "text", nullable: false),
                    size_in_bytes = table.Column<long>(type: "bigint", nullable: false),
                    sha256hash = table.Column<string>(type: "text", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_file_assets", x => x.id);
                    table.ForeignKey(
                        name: "fk_file_assets_users_uploader_id",
                        column: x => x.uploader_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_resource_progress_user_id_resource_id",
                table: "resource_progress",
                columns: new[] { "user_id", "resource_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_problem_test_cases_problem_id_order_index",
                table: "problem_test_cases",
                columns: new[] { "problem_id", "order_index" },
                unique: true,
                filter: "is_deleted = false");

            migrationBuilder.CreateIndex(
                name: "ix_courses_background_file_id",
                table: "courses",
                column: "background_file_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_courses_department_id",
                table: "courses",
                column: "department_id");

            migrationBuilder.CreateIndex(
                name: "ix_assignment_submissions_assignment_id_user_id",
                table: "assignment_submissions",
                columns: new[] { "assignment_id", "user_id" },
                unique: true,
                filter: "is_deleted = false");

            migrationBuilder.CreateIndex(
                name: "ix_assessment_attempts_assessment_id_user_id_attempt_number",
                table: "assessment_attempts",
                columns: new[] { "assessment_id", "user_id", "attempt_number" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_course_resources_module_id",
                table: "course_resources",
                column: "module_id");

            migrationBuilder.CreateIndex(
                name: "ix_file_assets_uploader_id",
                table: "file_assets",
                column: "uploader_id");

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
                name: "fk_courses_departments_department_id",
                table: "courses",
                column: "department_id",
                principalTable: "departments",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_courses_file_assets_background_file_id",
                table: "courses",
                column: "background_file_id",
                principalTable: "file_assets",
                principalColumn: "id");

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
                name: "fk_resource_progress_course_resources_resource_id",
                table: "resource_progress",
                column: "resource_id",
                principalTable: "course_resources",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_assessments_course_resources_resource_id",
                table: "assessments");

            migrationBuilder.DropForeignKey(
                name: "fk_assignments_course_resources_resource_id",
                table: "assignments");

            migrationBuilder.DropForeignKey(
                name: "fk_courses_departments_department_id",
                table: "courses");

            migrationBuilder.DropForeignKey(
                name: "fk_courses_file_assets_background_file_id",
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
                name: "fk_resource_progress_course_resources_resource_id",
                table: "resource_progress");

            migrationBuilder.DropTable(
                name: "course_resources");

            migrationBuilder.DropTable(
                name: "departments");

            migrationBuilder.DropTable(
                name: "file_assets");

            migrationBuilder.DropIndex(
                name: "ix_resource_progress_user_id_resource_id",
                table: "resource_progress");

            migrationBuilder.DropIndex(
                name: "ix_problem_test_cases_problem_id_order_index",
                table: "problem_test_cases");

            migrationBuilder.DropIndex(
                name: "ix_courses_background_file_id",
                table: "courses");

            migrationBuilder.DropIndex(
                name: "ix_courses_department_id",
                table: "courses");

            migrationBuilder.DropIndex(
                name: "ix_assignment_submissions_assignment_id_user_id",
                table: "assignment_submissions");

            migrationBuilder.DropIndex(
                name: "ix_assessment_attempts_assessment_id_user_id_attempt_number",
                table: "assessment_attempts");

            migrationBuilder.DropColumn(
                name: "difficulty",
                table: "problems");

            migrationBuilder.DropColumn(
                name: "order_index",
                table: "problem_submissions");

            migrationBuilder.DropColumn(
                name: "background_file_id",
                table: "courses");

            migrationBuilder.DropColumn(
                name: "code",
                table: "courses");

            migrationBuilder.DropColumn(
                name: "department_id",
                table: "courses");

            migrationBuilder.DropColumn(
                name: "section_id",
                table: "assessment_questions");

            migrationBuilder.RenameColumn(
                name: "activity_id",
                table: "resource_comments",
                newName: "resource_id");

            migrationBuilder.RenameIndex(
                name: "ix_resource_comments_activity_id",
                table: "resource_comments",
                newName: "ix_resource_comments_resource_id");

            migrationBuilder.RenameColumn(
                name: "memory_limit_kb",
                table: "problems",
                newName: "memory_limit_mb");

            migrationBuilder.RenameColumn(
                name: "instructions_md",
                table: "assignments",
                newName: "instructions_markdown");

            migrationBuilder.AddColumn<string>(
                name: "explanation",
                table: "problem_test_cases",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "thumbnail_url",
                table: "courses",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "module_resources",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    module_id = table.Column<long>(type: "bigint", nullable: false),
                    access_password = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    assessment_id = table.Column<long>(type: "bigint", nullable: true),
                    assignment_id = table.Column<long>(type: "bigint", nullable: true),
                    available_from = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    available_until = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    description = table.Column<string>(type: "text", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    is_published = table.Column<bool>(type: "boolean", nullable: false),
                    lesson_id = table.Column<long>(type: "bigint", nullable: true),
                    order_index = table.Column<int>(type: "integer", nullable: false),
                    problem_id = table.Column<long>(type: "bigint", nullable: true),
                    resource_type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_module_resources", x => x.id);
                    table.CheckConstraint("CK_ModuleResources_Polymorphic_ExactlyOne", "(\"lesson_id\" IS NOT NULL)::int + (\"assignment_id\" IS NOT NULL)::int + (\"assessment_id\" IS NOT NULL)::int + (\"problem_id\" IS NOT NULL)::int = 1");
                    table.ForeignKey(
                        name: "fk_module_resources_course_modules_module_id",
                        column: x => x.module_id,
                        principalTable: "modules",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_resource_progress_user_id_resource_id",
                table: "resource_progress",
                columns: new[] { "user_id", "resource_id" });

            migrationBuilder.CreateIndex(
                name: "ix_problem_test_cases_problem_id_order_index",
                table: "problem_test_cases",
                columns: new[] { "problem_id", "order_index" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_assignment_submissions_assignment_id_user_id",
                table: "assignment_submissions",
                columns: new[] { "assignment_id", "user_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_assessment_attempts_assessment_id_user_id_attempt_number",
                table: "assessment_attempts",
                columns: new[] { "assessment_id", "user_id", "attempt_number" });

            migrationBuilder.CreateIndex(
                name: "ix_module_resources_module_id",
                table: "module_resources",
                column: "module_id");

            migrationBuilder.AddForeignKey(
                name: "fk_assessments_module_resources_resource_id",
                table: "assessments",
                column: "resource_id",
                principalTable: "module_resources",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_assignments_module_resources_resource_id",
                table: "assignments",
                column: "resource_id",
                principalTable: "module_resources",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_lessons_module_resources_resource_id",
                table: "lessons",
                column: "resource_id",
                principalTable: "module_resources",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_problems_module_resources_resource_id",
                table: "problems",
                column: "resource_id",
                principalTable: "module_resources",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_resource_comments_module_resources_resource_id",
                table: "resource_comments",
                column: "resource_id",
                principalTable: "module_resources",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_resource_progress_module_resources_resource_id",
                table: "resource_progress",
                column: "resource_id",
                principalTable: "module_resources",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
