using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ToDoApi.Migrations
{
    /// <inheritdoc />
    public partial class AddIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_tasks_user_id",
                table: "tasks");

            migrationBuilder.RenameIndex(
                name: "ix_categories_user_id",
                table: "categories",
                newName: "idx_categories_user_id");

            migrationBuilder.CreateIndex(
                name: "idx_tasks__user_category_due",
                table: "tasks",
                columns: new[] { "user_id", "category_id", "due_date" });

            migrationBuilder.CreateIndex(
                name: "idx_tasks_user_due",
                table: "tasks",
                column: "due_date");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "idx_tasks__user_category_due",
                table: "tasks");

            migrationBuilder.DropIndex(
                name: "idx_tasks_user_due",
                table: "tasks");

            migrationBuilder.RenameIndex(
                name: "idx_categories_user_id",
                table: "categories",
                newName: "ix_categories_user_id");

            migrationBuilder.CreateIndex(
                name: "ix_tasks_user_id",
                table: "tasks",
                column: "user_id");
        }
    }
}
