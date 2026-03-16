using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ScholarFlow.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddStudentTeacherApprovalStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ReviewedAt",
                table: "StudentTeacherConnections",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "StudentTeacherConnections",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Pending");

            migrationBuilder.CreateIndex(
                name: "IX_StudentTeacherConnections_Status",
                table: "StudentTeacherConnections",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_StudentTeacherConnections_Status",
                table: "StudentTeacherConnections");

            migrationBuilder.DropColumn(
                name: "ReviewedAt",
                table: "StudentTeacherConnections");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "StudentTeacherConnections");
        }
    }
}
