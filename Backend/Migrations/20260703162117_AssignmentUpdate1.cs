using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class AssignmentUpdate1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_users_username_email",
                table: "users");

            migrationBuilder.DropIndex(
                name: "ix_resource_progress_user_id_resource_id_is_completed",
                table: "resource_progress");

            migrationBuilder.DropIndex(
                name: "ix_modules_course_id_order_index",
                table: "modules");

            migrationBuilder.DropIndex(
                name: "ix_module_resources_module_id",
                table: "module_resources");

            migrationBuilder.DropIndex(
                name: "ix_enrollments_course_id_user_id",
                table: "enrollments");

            migrationBuilder.DropIndex(
                name: "ix_assessment_questions_assessment_id_order_index",
                table: "assessment_questions");

            migrationBuilder.DropIndex(
                name: "ix_assessment_attempts_assessment_id_user_id",
                table: "assessment_attempts");

            migrationBuilder.DropColumn(
                name: "can_resubmit",
                table: "assignment_grades");

            migrationBuilder.AddColumn<long>(
                name: "assessment_id",
                table: "module_resources",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "assignment_id",
                table: "module_resources",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "lesson_id",
                table: "module_resources",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "problem_id",
                table: "module_resources",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "max_attempt",
                table: "assignments",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_users_email",
                table: "users",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_users_username",
                table: "users",
                column: "username",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_resource_progress_user_id_resource_id",
                table: "resource_progress",
                columns: new[] { "user_id", "resource_id" });

            migrationBuilder.CreateIndex(
                name: "ix_modules_course_id_order_index",
                table: "modules",
                columns: new[] { "course_id", "order_index" },
                unique: true,
                filter: "is_deleted = false");

            migrationBuilder.CreateIndex(
                name: "ix_module_resources_module_id",
                table: "module_resources",
                column: "module_id");

            migrationBuilder.AddCheckConstraint(
                name: "CK_ModuleResources_Polymorphic_ExactlyOne",
                table: "module_resources",
                sql: "(\"lesson_id\" IS NOT NULL)::int + (\"assignment_id\" IS NOT NULL)::int + (\"assessment_id\" IS NOT NULL)::int + (\"problem_id\" IS NOT NULL)::int = 1");

            migrationBuilder.CreateIndex(
                name: "ix_enrollments_course_id_user_id",
                table: "enrollments",
                columns: new[] { "course_id", "user_id" },
                unique: true,
                filter: "is_deleted = false");

            migrationBuilder.CreateIndex(
                name: "ix_assessment_questions_assessment_id_order_index",
                table: "assessment_questions",
                columns: new[] { "assessment_id", "order_index" },
                unique: true,
                filter: "is_deleted = false");

            migrationBuilder.CreateIndex(
                name: "ix_assessment_attempts_assessment_id_user_id_attempt_number",
                table: "assessment_attempts",
                columns: new[] { "assessment_id", "user_id", "attempt_number" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_users_email",
                table: "users");

            migrationBuilder.DropIndex(
                name: "ix_users_username",
                table: "users");

            migrationBuilder.DropIndex(
                name: "ix_resource_progress_user_id_resource_id",
                table: "resource_progress");

            migrationBuilder.DropIndex(
                name: "ix_modules_course_id_order_index",
                table: "modules");

            migrationBuilder.DropIndex(
                name: "ix_module_resources_module_id",
                table: "module_resources");

            migrationBuilder.DropCheckConstraint(
                name: "CK_ModuleResources_Polymorphic_ExactlyOne",
                table: "module_resources");

            migrationBuilder.DropIndex(
                name: "ix_enrollments_course_id_user_id",
                table: "enrollments");

            migrationBuilder.DropIndex(
                name: "ix_assessment_questions_assessment_id_order_index",
                table: "assessment_questions");

            migrationBuilder.DropIndex(
                name: "ix_assessment_attempts_assessment_id_user_id_attempt_number",
                table: "assessment_attempts");

            migrationBuilder.DropColumn(
                name: "assessment_id",
                table: "module_resources");

            migrationBuilder.DropColumn(
                name: "assignment_id",
                table: "module_resources");

            migrationBuilder.DropColumn(
                name: "lesson_id",
                table: "module_resources");

            migrationBuilder.DropColumn(
                name: "problem_id",
                table: "module_resources");

            migrationBuilder.DropColumn(
                name: "max_attempt",
                table: "assignments");

            migrationBuilder.AddColumn<bool>(
                name: "can_resubmit",
                table: "assignment_grades",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "ix_users_username_email",
                table: "users",
                columns: new[] { "username", "email" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_resource_progress_user_id_resource_id_is_completed",
                table: "resource_progress",
                columns: new[] { "user_id", "resource_id", "is_completed" });

            migrationBuilder.CreateIndex(
                name: "ix_modules_course_id_order_index",
                table: "modules",
                columns: new[] { "course_id", "order_index" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_module_resources_module_id",
                table: "module_resources",
                column: "module_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_enrollments_course_id_user_id",
                table: "enrollments",
                columns: new[] { "course_id", "user_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_assessment_questions_assessment_id_order_index",
                table: "assessment_questions",
                columns: new[] { "assessment_id", "order_index" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_assessment_attempts_assessment_id_user_id",
                table: "assessment_attempts",
                columns: new[] { "assessment_id", "user_id" });
        }
    }
}
