using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Backend.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class JudgeImpl : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "language",
                table: "problems");

            migrationBuilder.DropColumn(
                name: "status",
                table: "problem_test_results");

            migrationBuilder.DropColumn(
                name: "execution_time_ms",
                table: "problem_submissions");

            migrationBuilder.DropColumn(
                name: "language",
                table: "problem_submissions");

            migrationBuilder.DropColumn(
                name: "memory_used_kb",
                table: "problem_submissions");

            migrationBuilder.DropColumn(
                name: "passed_test_cases",
                table: "problem_submissions");

            migrationBuilder.DropColumn(
                name: "total_test_cases",
                table: "problem_submissions");

            migrationBuilder.RenameColumn(
                name: "error_message",
                table: "problem_test_results",
                newName: "runtime_output");

            migrationBuilder.RenameColumn(
                name: "actual_output",
                table: "problem_test_results",
                newName: "runtime_error");

            migrationBuilder.AlterColumn<double>(
                name: "execution_time_ms",
                table: "problem_test_results",
                type: "double precision",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_accepted",
                table: "problem_test_results",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "compile_error",
                table: "problem_submissions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "profile_id",
                table: "problem_submissions",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AlterColumn<double>(
                name: "execution_time_ms",
                table: "code_execution_logs",
                type: "double precision",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "compilers",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "text", nullable: false),
                    language = table.Column<int>(type: "integer", nullable: false),
                    source_file_name = table.Column<string>(type: "text", nullable: false),
                    application_name = table.Column<string>(type: "text", nullable: false),
                    version = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_compilers", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "compiler_profiles",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    problem_id = table.Column<long>(type: "bigint", nullable: false),
                    compiler_id = table.Column<long>(type: "bigint", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    compile_cmd = table.Column<string>(type: "text", nullable: false),
                    run_cmd = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_compiler_profiles", x => x.id);
                    table.ForeignKey(
                        name: "fk_compiler_profiles_coding_problems_problem_id",
                        column: x => x.problem_id,
                        principalTable: "problems",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_compiler_profiles_compilers_compiler_id",
                        column: x => x.compiler_id,
                        principalTable: "compilers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_problem_submissions_profile_id",
                table: "problem_submissions",
                column: "profile_id");

            migrationBuilder.CreateIndex(
                name: "ix_compiler_profiles_compiler_id",
                table: "compiler_profiles",
                column: "compiler_id");

            migrationBuilder.CreateIndex(
                name: "ix_compiler_profiles_problem_id",
                table: "compiler_profiles",
                column: "problem_id");

            migrationBuilder.CreateIndex(
                name: "ix_compilers_name",
                table: "compilers",
                column: "name",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "fk_problem_submissions_compiler_profiles_profile_id",
                table: "problem_submissions",
                column: "profile_id",
                principalTable: "compiler_profiles",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_problem_submissions_compiler_profiles_profile_id",
                table: "problem_submissions");

            migrationBuilder.DropTable(
                name: "compiler_profiles");

            migrationBuilder.DropTable(
                name: "compilers");

            migrationBuilder.DropIndex(
                name: "ix_problem_submissions_profile_id",
                table: "problem_submissions");

            migrationBuilder.DropColumn(
                name: "is_accepted",
                table: "problem_test_results");

            migrationBuilder.DropColumn(
                name: "compile_error",
                table: "problem_submissions");

            migrationBuilder.DropColumn(
                name: "profile_id",
                table: "problem_submissions");

            migrationBuilder.RenameColumn(
                name: "runtime_output",
                table: "problem_test_results",
                newName: "error_message");

            migrationBuilder.RenameColumn(
                name: "runtime_error",
                table: "problem_test_results",
                newName: "actual_output");

            migrationBuilder.AddColumn<string>(
                name: "language",
                table: "problems",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<int>(
                name: "execution_time_ms",
                table: "problem_test_results",
                type: "integer",
                nullable: true,
                oldClrType: typeof(double),
                oldType: "double precision",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "status",
                table: "problem_test_results",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "execution_time_ms",
                table: "problem_submissions",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "language",
                table: "problem_submissions",
                type: "integer",
                maxLength: 20,
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "memory_used_kb",
                table: "problem_submissions",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "passed_test_cases",
                table: "problem_submissions",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "total_test_cases",
                table: "problem_submissions",
                type: "integer",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "execution_time_ms",
                table: "code_execution_logs",
                type: "integer",
                nullable: true,
                oldClrType: typeof(double),
                oldType: "double precision",
                oldNullable: true);
        }
    }
}
