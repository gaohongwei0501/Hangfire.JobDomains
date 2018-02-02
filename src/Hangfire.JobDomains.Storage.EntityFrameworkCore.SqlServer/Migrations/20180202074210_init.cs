using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace Hangfire.PluginPackets.Storage.EntityFrameworkCore.SqlServer.Migrations
{
    public partial class init : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "Hangfire");

            migrationBuilder.CreateTable(
                name: "Extension_Assembly",
                schema: "Hangfire",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    CreatedAt = table.Column<DateTime>(nullable: false),
                    Description = table.Column<string>(maxLength: 200, nullable: true),
                    FileName = table.Column<string>(maxLength: 200, nullable: false),
                    FullName = table.Column<string>(maxLength: 200, nullable: false),
                    PluginId = table.Column<int>(nullable: false),
                    ShortName = table.Column<string>(maxLength: 50, nullable: false),
                    Title = table.Column<string>(maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Extension_Assembly", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Extension_Job",
                schema: "Hangfire",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    AssemblyId = table.Column<int>(nullable: false),
                    CreatedAt = table.Column<DateTime>(nullable: false),
                    Description = table.Column<string>(maxLength: 200, nullable: true),
                    FullName = table.Column<string>(maxLength: 200, nullable: false),
                    Name = table.Column<string>(maxLength: 50, nullable: false),
                    PluginId = table.Column<int>(nullable: false),
                    Title = table.Column<string>(maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Extension_Job", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Extension_JobConstructorParameter",
                schema: "Hangfire",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    AssemblyId = table.Column<int>(nullable: false),
                    ConstructorGuid = table.Column<string>(maxLength: 50, nullable: false),
                    CreatedAt = table.Column<DateTime>(nullable: false),
                    JobId = table.Column<int>(nullable: false),
                    Name = table.Column<string>(maxLength: 50, nullable: false),
                    PluginId = table.Column<int>(nullable: false),
                    Type = table.Column<string>(maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Extension_JobConstructorParameter", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Extension_Plugin",
                schema: "Hangfire",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    CreatedAt = table.Column<DateTime>(nullable: false),
                    Description = table.Column<string>(maxLength: 200, nullable: true),
                    PathName = table.Column<string>(nullable: true),
                    Title = table.Column<string>(maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Extension_Plugin", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Extension_Queue",
                schema: "Hangfire",
                columns: table => new
                {
                    Name = table.Column<string>(nullable: false),
                    CreatedAt = table.Column<DateTime>(nullable: false),
                    Description = table.Column<string>(maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Extension_Queue", x => x.Name);
                });

            migrationBuilder.CreateTable(
                name: "Extension_Server",
                schema: "Hangfire",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    CreatedAt = table.Column<DateTime>(nullable: false),
                    Description = table.Column<string>(maxLength: 200, nullable: true),
                    Name = table.Column<string>(maxLength: 50, nullable: false),
                    PlugPath = table.Column<string>(maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Extension_Server", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Extension_ServerPlugin",
                schema: "Hangfire",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    CreatedAt = table.Column<DateTime>(nullable: false),
                    PlugName = table.Column<string>(maxLength: 50, nullable: false),
                    ServerName = table.Column<string>(maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Extension_ServerPlugin", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Extension_ServerQueue",
                schema: "Hangfire",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    CreatedAt = table.Column<DateTime>(nullable: false),
                    QueueName = table.Column<string>(maxLength: 50, nullable: false),
                    ServerName = table.Column<string>(maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Extension_ServerQueue", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Extension_Assembly",
                schema: "Hangfire");

            migrationBuilder.DropTable(
                name: "Extension_Job",
                schema: "Hangfire");

            migrationBuilder.DropTable(
                name: "Extension_JobConstructorParameter",
                schema: "Hangfire");

            migrationBuilder.DropTable(
                name: "Extension_Plugin",
                schema: "Hangfire");

            migrationBuilder.DropTable(
                name: "Extension_Queue",
                schema: "Hangfire");

            migrationBuilder.DropTable(
                name: "Extension_Server",
                schema: "Hangfire");

            migrationBuilder.DropTable(
                name: "Extension_ServerPlugin",
                schema: "Hangfire");

            migrationBuilder.DropTable(
                name: "Extension_ServerQueue",
                schema: "Hangfire");
        }
    }
}
