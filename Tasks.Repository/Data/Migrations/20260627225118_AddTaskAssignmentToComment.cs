using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tasks.Repository.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddTaskAssignmentToComment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TaskAssignmentId",
                table: "TaskComments",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_TaskComments_TaskAssignmentId",
                table: "TaskComments",
                column: "TaskAssignmentId");

            migrationBuilder.AddForeignKey(
                name: "FK_TaskComments_TaskAssignments_TaskAssignmentId",
                table: "TaskComments",
                column: "TaskAssignmentId",
                principalTable: "TaskAssignments",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TaskComments_TaskAssignments_TaskAssignmentId",
                table: "TaskComments");

            migrationBuilder.DropIndex(
                name: "IX_TaskComments_TaskAssignmentId",
                table: "TaskComments");

            migrationBuilder.DropColumn(
                name: "TaskAssignmentId",
                table: "TaskComments");
        }
    }
}
