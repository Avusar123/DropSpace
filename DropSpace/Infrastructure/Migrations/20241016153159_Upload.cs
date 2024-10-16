using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DropSpace.Migrations
{
    /// <inheritdoc />
    public partial class Upload : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PendingUploads");

            migrationBuilder.AddColumn<bool>(
                name: "IsUploaded",
                table: "Files",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsUploaded",
                table: "Files");

            migrationBuilder.CreateTable(
                name: "PendingUploads",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FileId = table.Column<Guid>(type: "uuid", nullable: false),
                    ChunkSize = table.Column<long>(type: "bigint", nullable: false),
                    IsCompleted = table.Column<bool>(type: "boolean", nullable: false),
                    LastChunkUploaded = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    SendedSize = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PendingUploads", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PendingUploads_Files_FileId",
                        column: x => x.FileId,
                        principalTable: "Files",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PendingUploads_FileId",
                table: "PendingUploads",
                column: "FileId",
                unique: true);
        }
    }
}
