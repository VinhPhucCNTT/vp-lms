using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddedCourseUniquenessIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_courses_instructor_id_title",
                table: "courses");

            migrationBuilder.CreateIndex(
                name: "ix_courses_instructor_id_title",
                table: "courses",
                columns: new[] { "instructor_id", "title" },
                unique: true,
                filter: "is_deleted = false");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_courses_instructor_id_title",
                table: "courses");

            migrationBuilder.CreateIndex(
                name: "ix_courses_instructor_id_title",
                table: "courses",
                columns: new[] { "instructor_id", "title" });
        }
    }
}
