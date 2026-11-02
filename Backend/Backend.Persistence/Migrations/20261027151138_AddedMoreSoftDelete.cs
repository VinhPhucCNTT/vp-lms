using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddedMoreSoftDelete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_problems_resource_id",
                table: "problems");

            migrationBuilder.DropIndex(
                name: "ix_problem_test_cases_problem_id_order_index",
                table: "problem_test_cases");

            migrationBuilder.DropIndex(
                name: "ix_lessons_resource_id",
                table: "lessons");

            migrationBuilder.DropIndex(
                name: "ix_courses_background_file_id",
                table: "courses");

            migrationBuilder.DropIndex(
                name: "ix_attempt_answers_attempt_question_id",
                table: "attempt_answers");

            migrationBuilder.DropIndex(
                name: "ix_attempt_answers_grader_id_attempt_question_id",
                table: "attempt_answers");

            migrationBuilder.DropIndex(
                name: "ix_assignments_resource_id",
                table: "assignments");

            migrationBuilder.DropIndex(
                name: "ix_assignment_grades_submission_id",
                table: "assignment_grades");

            migrationBuilder.DropIndex(
                name: "ix_assignment_files_submission_id_file_id_order_index",
                table: "assignment_files");

            migrationBuilder.DropIndex(
                name: "ix_assessments_resource_id",
                table: "assessments");

            migrationBuilder.DropIndex(
                name: "ix_assessment_questions_assessment_id_order_index",
                table: "assessment_questions");

            migrationBuilder.DropIndex(
                name: "ix_assessment_attempts_assessment_id_student_id_attempt_number",
                table: "assessment_attempts");

            migrationBuilder.AddColumn<DateTime>(
                name: "deleted_at",
                table: "problem_test_results",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_deleted",
                table: "problem_test_results",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "deleted_at",
                table: "problem_test_cases",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_deleted",
                table: "problem_test_cases",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "deleted_at",
                table: "problem_submissions",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_deleted",
                table: "problem_submissions",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "deleted_at",
                table: "compiler_profiles",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_deleted",
                table: "compiler_profiles",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "deleted_at",
                table: "attempt_questions",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_deleted",
                table: "attempt_questions",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "deleted_at",
                table: "attempt_answers",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_deleted",
                table: "attempt_answers",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "deleted_at",
                table: "assignment_grades",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_deleted",
                table: "assignment_grades",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "deleted_at",
                table: "assignment_files",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_deleted",
                table: "assignment_files",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "deleted_at",
                table: "assessment_questions",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_deleted",
                table: "assessment_questions",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "ix_problems_resource_id",
                table: "problems",
                column: "resource_id",
                unique: true,
                filter: "is_deleted = false");

            migrationBuilder.CreateIndex(
                name: "ix_problem_test_cases_problem_id_order_index",
                table: "problem_test_cases",
                columns: new[] { "problem_id", "order_index" },
                unique: true,
                filter: "is_deleted = false");

            migrationBuilder.CreateIndex(
                name: "ix_lessons_resource_id",
                table: "lessons",
                column: "resource_id",
                unique: true,
                filter: "is_deleted = false");

            migrationBuilder.CreateIndex(
                name: "ix_courses_background_file_id",
                table: "courses",
                column: "background_file_id",
                unique: true,
                filter: "is_deleted = false");

            migrationBuilder.CreateIndex(
                name: "ix_attempt_answers_attempt_question_id",
                table: "attempt_answers",
                column: "attempt_question_id",
                unique: true,
                filter: "is_deleted = false");

            migrationBuilder.CreateIndex(
                name: "ix_attempt_answers_grader_id_attempt_question_id",
                table: "attempt_answers",
                columns: new[] { "grader_id", "attempt_question_id" },
                unique: true,
                filter: "is_deleted = false");

            migrationBuilder.CreateIndex(
                name: "ix_assignments_resource_id",
                table: "assignments",
                column: "resource_id",
                unique: true,
                filter: "is_deleted = false");

            migrationBuilder.CreateIndex(
                name: "ix_assignment_grades_submission_id",
                table: "assignment_grades",
                column: "submission_id",
                unique: true,
                filter: "is_deleted = false");

            migrationBuilder.CreateIndex(
                name: "ix_assignment_files_submission_id_file_id_order_index",
                table: "assignment_files",
                columns: new[] { "submission_id", "file_id", "order_index" },
                unique: true,
                filter: "is_deleted = false");

            migrationBuilder.CreateIndex(
                name: "ix_assessments_resource_id",
                table: "assessments",
                column: "resource_id",
                unique: true,
                filter: "is_deleted = false");

            migrationBuilder.CreateIndex(
                name: "ix_assessment_questions_assessment_id_order_index",
                table: "assessment_questions",
                columns: new[] { "assessment_id", "order_index" },
                unique: true,
                filter: "is_deleted = false");

            migrationBuilder.CreateIndex(
                name: "ix_assessment_attempts_assessment_id_student_id_attempt_number",
                table: "assessment_attempts",
                columns: new[] { "assessment_id", "student_id", "attempt_number" },
                unique: true,
                filter: "is_deleted = false");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_problems_resource_id",
                table: "problems");

            migrationBuilder.DropIndex(
                name: "ix_problem_test_cases_problem_id_order_index",
                table: "problem_test_cases");

            migrationBuilder.DropIndex(
                name: "ix_lessons_resource_id",
                table: "lessons");

            migrationBuilder.DropIndex(
                name: "ix_courses_background_file_id",
                table: "courses");

            migrationBuilder.DropIndex(
                name: "ix_attempt_answers_attempt_question_id",
                table: "attempt_answers");

            migrationBuilder.DropIndex(
                name: "ix_attempt_answers_grader_id_attempt_question_id",
                table: "attempt_answers");

            migrationBuilder.DropIndex(
                name: "ix_assignments_resource_id",
                table: "assignments");

            migrationBuilder.DropIndex(
                name: "ix_assignment_grades_submission_id",
                table: "assignment_grades");

            migrationBuilder.DropIndex(
                name: "ix_assignment_files_submission_id_file_id_order_index",
                table: "assignment_files");

            migrationBuilder.DropIndex(
                name: "ix_assessments_resource_id",
                table: "assessments");

            migrationBuilder.DropIndex(
                name: "ix_assessment_questions_assessment_id_order_index",
                table: "assessment_questions");

            migrationBuilder.DropIndex(
                name: "ix_assessment_attempts_assessment_id_student_id_attempt_number",
                table: "assessment_attempts");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                table: "problem_test_results");

            migrationBuilder.DropColumn(
                name: "is_deleted",
                table: "problem_test_results");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                table: "problem_test_cases");

            migrationBuilder.DropColumn(
                name: "is_deleted",
                table: "problem_test_cases");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                table: "problem_submissions");

            migrationBuilder.DropColumn(
                name: "is_deleted",
                table: "problem_submissions");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                table: "compiler_profiles");

            migrationBuilder.DropColumn(
                name: "is_deleted",
                table: "compiler_profiles");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                table: "attempt_questions");

            migrationBuilder.DropColumn(
                name: "is_deleted",
                table: "attempt_questions");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                table: "attempt_answers");

            migrationBuilder.DropColumn(
                name: "is_deleted",
                table: "attempt_answers");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                table: "assignment_grades");

            migrationBuilder.DropColumn(
                name: "is_deleted",
                table: "assignment_grades");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                table: "assignment_files");

            migrationBuilder.DropColumn(
                name: "is_deleted",
                table: "assignment_files");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                table: "assessment_questions");

            migrationBuilder.DropColumn(
                name: "is_deleted",
                table: "assessment_questions");

            migrationBuilder.CreateIndex(
                name: "ix_problems_resource_id",
                table: "problems",
                column: "resource_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_problem_test_cases_problem_id_order_index",
                table: "problem_test_cases",
                columns: new[] { "problem_id", "order_index" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_lessons_resource_id",
                table: "lessons",
                column: "resource_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_courses_background_file_id",
                table: "courses",
                column: "background_file_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_attempt_answers_attempt_question_id",
                table: "attempt_answers",
                column: "attempt_question_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_attempt_answers_grader_id_attempt_question_id",
                table: "attempt_answers",
                columns: new[] { "grader_id", "attempt_question_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_assignments_resource_id",
                table: "assignments",
                column: "resource_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_assignment_grades_submission_id",
                table: "assignment_grades",
                column: "submission_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_assignment_files_submission_id_file_id_order_index",
                table: "assignment_files",
                columns: new[] { "submission_id", "file_id", "order_index" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_assessments_resource_id",
                table: "assessments",
                column: "resource_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_assessment_questions_assessment_id_order_index",
                table: "assessment_questions",
                columns: new[] { "assessment_id", "order_index" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_assessment_attempts_assessment_id_student_id_attempt_number",
                table: "assessment_attempts",
                columns: new[] { "assessment_id", "student_id", "attempt_number" },
                unique: true);
        }
    }
}
