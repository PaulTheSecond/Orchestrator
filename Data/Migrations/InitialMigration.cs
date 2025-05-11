using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OrchestratorApp.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ProcedureTemplates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Version = table.Column<int>(type: "integer", nullable: false),
                    IsPublished = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProcedureTemplates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ContestTemplates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProcedureTemplateId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Version = table.Column<int>(type: "integer", nullable: false),
                    IsPublished = table.Column<bool>(type: "boolean", nullable: false),
                    StatusModelJson = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContestTemplates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ContestTemplates_ProcedureTemplates_ProcedureTemplateId",
                        column: x => x.ProcedureTemplateId,
                        principalTable: "ProcedureTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProcedureStageTemplates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProcedureTemplateId = table.Column<Guid>(type: "uuid", nullable: false),
                    StageType = table.Column<int>(type: "integer", nullable: false),
                    Order = table.Column<int>(type: "integer", nullable: false),
                    PreviousStageId = table.Column<Guid>(type: "uuid", nullable: true),
                    NextStageId = table.Column<Guid>(type: "uuid", nullable: true),
                    DefaultServiceName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProcedureStageTemplates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProcedureStageTemplates_ProcedureStageTemplates_PreviousStageId",
                        column: x => x.PreviousStageId,
                        principalTable: "ProcedureStageTemplates",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ProcedureStageTemplates_ProcedureTemplates_ProcedureTemplateId",
                        column: x => x.ProcedureTemplateId,
                        principalTable: "ProcedureTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StageTemplates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ContestTemplateId = table.Column<Guid>(type: "uuid", nullable: false),
                    StageType = table.Column<int>(type: "integer", nullable: false),
                    Order = table.Column<int>(type: "integer", nullable: false),
                    PreviousStageId = table.Column<Guid>(type: "uuid", nullable: true),
                    NextStageId = table.Column<Guid>(type: "uuid", nullable: true),
                    DefaultServiceName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StageTemplates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StageTemplates_ContestTemplates_ContestTemplateId",
                        column: x => x.ContestTemplateId,
                        principalTable: "ContestTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StageTemplates_StageTemplates_PreviousStageId",
                        column: x => x.PreviousStageId,
                        principalTable: "StageTemplates",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ProcedureInstances",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProcedureTemplateId = table.Column<Guid>(type: "uuid", nullable: false),
                    TemplateVersion = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    CurrentStageId = table.Column<Guid>(type: "uuid", nullable: true),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProcedureInstances", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProcedureInstances_ProcedureStageTemplates_CurrentStageId",
                        column: x => x.CurrentStageId,
                        principalTable: "ProcedureStageTemplates",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ProcedureInstances_ProcedureTemplates_ProcedureTemplateId",
                        column: x => x.ProcedureTemplateId,
                        principalTable: "ProcedureTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ContestInstances",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProcedureInstanceId = table.Column<Guid>(type: "uuid", nullable: false),
                    ContestTemplateId = table.Column<Guid>(type: "uuid", nullable: false),
                    ContestTemplateVersion = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    CurrentStageId = table.Column<Guid>(type: "uuid", nullable: true),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    EndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContestInstances", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ContestInstances_ContestTemplates_ContestTemplateId",
                        column: x => x.ContestTemplateId,
                        principalTable: "ContestTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ContestInstances_ProcedureInstances_ProcedureInstanceId",
                        column: x => x.ProcedureInstanceId,
                        principalTable: "ProcedureInstances",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ContestInstances_StageTemplates_CurrentStageId",
                        column: x => x.CurrentStageId,
                        principalTable: "StageTemplates",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ApplicationInstances",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ContestInstanceId = table.Column<Guid>(type: "uuid", nullable: false),
                    CurrentStageId = table.Column<Guid>(type: "uuid", nullable: true),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ExternalApplicationId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApplicationInstances", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ApplicationInstances_ContestInstances_ContestInstanceId",
                        column: x => x.ContestInstanceId,
                        principalTable: "ContestInstances",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ApplicationInstances_StageTemplates_CurrentStageId",
                        column: x => x.CurrentStageId,
                        principalTable: "StageTemplates",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "StageConfigurations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ContestInstanceId = table.Column<Guid>(type: "uuid", nullable: false),
                    StageTemplateId = table.Column<Guid>(type: "uuid", nullable: false),
                    StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    EndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ServiceName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StageConfigurations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StageConfigurations_ContestInstances_ContestInstanceId",
                        column: x => x.ContestInstanceId,
                        principalTable: "ContestInstances",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StageConfigurations_StageTemplates_StageTemplateId",
                        column: x => x.StageTemplateId,
                        principalTable: "StageTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "StageResults",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ApplicationInstanceId = table.Column<Guid>(type: "uuid", nullable: false),
                    StageTemplateId = table.Column<Guid>(type: "uuid", nullable: false),
                    ResultStatus = table.Column<int>(type: "integer", nullable: false),
                    ResultData = table.Column<string>(type: "text", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IntegrationEventId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StageResults", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StageResults_ApplicationInstances_ApplicationInstanceId",
                        column: x => x.ApplicationInstanceId,
                        principalTable: "ApplicationInstances",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StageResults_StageTemplates_StageTemplateId",
                        column: x => x.StageTemplateId,
                        principalTable: "StageTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationInstances_ContestInstanceId",
                table: "ApplicationInstances",
                column: "ContestInstanceId");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationInstances_CurrentStageId",
                table: "ApplicationInstances",
                column: "CurrentStageId");

            migrationBuilder.CreateIndex(
                name: "IX_ContestInstances_ContestTemplateId",
                table: "ContestInstances",
                column: "ContestTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_ContestInstances_CurrentStageId",
                table: "ContestInstances",
                column: "CurrentStageId");

            migrationBuilder.CreateIndex(
                name: "IX_ContestInstances_ProcedureInstanceId",
                table: "ContestInstances",
                column: "ProcedureInstanceId");

            migrationBuilder.CreateIndex(
                name: "IX_ContestTemplates_Name_IsPublished",
                table: "ContestTemplates",
                columns: new[] { "Name", "IsPublished" },
                filter: "\"IsPublished\" = true",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ContestTemplates_ProcedureTemplateId",
                table: "ContestTemplates",
                column: "ProcedureTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_ProcedureInstances_CurrentStageId",
                table: "ProcedureInstances",
                column: "CurrentStageId");

            migrationBuilder.CreateIndex(
                name: "IX_ProcedureInstances_ProcedureTemplateId",
                table: "ProcedureInstances",
                column: "ProcedureTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_ProcedureStageTemplates_PreviousStageId",
                table: "ProcedureStageTemplates",
                column: "PreviousStageId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProcedureStageTemplates_ProcedureTemplateId",
                table: "ProcedureStageTemplates",
                column: "ProcedureTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_ProcedureTemplates_Name_IsPublished",
                table: "ProcedureTemplates",
                columns: new[] { "Name", "IsPublished" },
                filter: "\"IsPublished\" = true",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StageConfigurations_ContestInstanceId",
                table: "StageConfigurations",
                column: "ContestInstanceId");

            migrationBuilder.CreateIndex(
                name: "IX_StageConfigurations_StageTemplateId",
                table: "StageConfigurations",
                column: "StageTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_StageResults_ApplicationInstanceId",
                table: "StageResults",
                column: "ApplicationInstanceId");

            migrationBuilder.CreateIndex(
                name: "IX_StageResults_StageTemplateId",
                table: "StageResults",
                column: "StageTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_StageTemplates_ContestTemplateId",
                table: "StageTemplates",
                column: "ContestTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_StageTemplates_PreviousStageId",
                table: "StageTemplates",
                column: "PreviousStageId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StageConfigurations");

            migrationBuilder.DropTable(
                name: "StageResults");

            migrationBuilder.DropTable(
                name: "ApplicationInstances");

            migrationBuilder.DropTable(
                name: "ContestInstances");

            migrationBuilder.DropTable(
                name: "ProcedureInstances");

            migrationBuilder.DropTable(
                name: "StageTemplates");

            migrationBuilder.DropTable(
                name: "ProcedureStageTemplates");

            migrationBuilder.DropTable(
                name: "ContestTemplates");

            migrationBuilder.DropTable(
                name: "ProcedureTemplates");
        }
    }
}
