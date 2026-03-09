using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ScholarFlow.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateStreamSubjectToAuditableEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "StreamSubjects",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                table: "StreamSubjects",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "StreamSubjects",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DeletedBy",
                table: "StreamSubjects",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "StreamSubjects",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "StreamSubjects",
                type: "varbinary(max)",
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "StreamSubjects",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedBy",
                table: "StreamSubjects",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_StreamSubjects_StreamId_SubjectId",
                table: "StreamSubjects",
                columns: new[] { "StreamId", "SubjectId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_StreamSubjects_StreamId_SubjectId",
                table: "StreamSubjects");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "StreamSubjects");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "StreamSubjects");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "StreamSubjects");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "StreamSubjects");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "StreamSubjects");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "StreamSubjects");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "StreamSubjects");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "StreamSubjects");
        }
    }
}
