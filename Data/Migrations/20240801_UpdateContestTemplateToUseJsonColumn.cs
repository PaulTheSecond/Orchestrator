
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OrchestratorApp.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateContestTemplateToUseJsonColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Добавляем новую колонку JSON
            migrationBuilder.AddColumn<string[]>(
                name: "StatusModelArray",
                table: "ContestTemplates",
                type: "jsonb",
                nullable: false,
                defaultValueSql: "'[]'::jsonb");

            // Копируем данные из StatusModel в StatusModelArray (преобразование из строки в jsonb)
            migrationBuilder.Sql(@"
                UPDATE ""ContestTemplates"" 
                SET ""StatusModelArray"" = 
                    CASE 
                        WHEN ""StatusModel"" IS NULL OR ""StatusModel"" = '' THEN '[]'::jsonb
                        ELSE ""StatusModel""::jsonb
                    END
            ");

            // Удаляем старую колонку
            migrationBuilder.DropColumn(
                name: "StatusModel",
                table: "ContestTemplates");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Добавляем обратно старую колонку
            migrationBuilder.AddColumn<string>(
                name: "StatusModel",
                table: "ContestTemplates",
                type: "text",
                nullable: false,
                defaultValue: "[]");

            // Копируем данные обратно (преобразуем jsonb в строку)
            migrationBuilder.Sql(@"
                UPDATE ""ContestTemplates"" 
                SET ""StatusModel"" = 
                    CASE
                        WHEN ""StatusModelArray"" IS NULL THEN '[]'
                        ELSE ""StatusModelArray""::text
                    END
            ");

            // Удаляем новую колонку
            migrationBuilder.DropColumn(
                name: "StatusModelArray",
                table: "ContestTemplates");
        }
    }
}
