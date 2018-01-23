using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace Hangfire.JobDomains.Storage.EntityFrameworkCore.SqlServer.Migrations
{
    public partial class init : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "Hangfire");

            migrationBuilder.CreateTable(
                name: "JobDomains.Assembly",
                schema: "Hangfire",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    CreatedAt = table.Column<DateTime>(nullable: false),
                    Description = table.Column<string>(maxLength: 200, nullable: true),
                    DomainID = table.Column<int>(nullable: false),
                    FileName = table.Column<string>(maxLength: 200, nullable: false),
                    FullName = table.Column<string>(maxLength: 200, nullable: false),
                    ShortName = table.Column<string>(maxLength: 50, nullable: false),
                    Title = table.Column<string>(maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JobDomains.Assembly", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "JobDomains.Domain",
                schema: "Hangfire",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    CreatedAt = table.Column<DateTime>(nullable: false),
                    Description = table.Column<string>(maxLength: 200, nullable: true),
                    PathName = table.Column<string>(nullable: true),
                    Title = table.Column<string>(maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JobDomains.Domain", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "JobDomains.Job",
                schema: "Hangfire",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    AssemblyID = table.Column<int>(nullable: false),
                    CreatedAt = table.Column<DateTime>(nullable: false),
                    Description = table.Column<string>(maxLength: 200, nullable: true),
                    DomainID = table.Column<int>(nullable: false),
                    FullName = table.Column<string>(maxLength: 200, nullable: false),
                    Name = table.Column<string>(maxLength: 50, nullable: false),
                    Title = table.Column<string>(maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JobDomains.Job", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "JobDomains.JobConstructorParameter",
                schema: "Hangfire",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    AssemblyID = table.Column<int>(nullable: false),
                    ConstructorGuid = table.Column<string>(maxLength: 50, nullable: false),
                    CreatedAt = table.Column<DateTime>(nullable: false),
                    DomainID = table.Column<int>(nullable: false),
                    JobID = table.Column<int>(nullable: false),
                    Name = table.Column<string>(maxLength: 50, nullable: false),
                    Type = table.Column<string>(maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JobDomains.JobConstructorParameter", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "JobDomains.Queue",
                schema: "Hangfire",
                columns: table => new
                {
                    Name = table.Column<string>(nullable: false),
                    CreatedAt = table.Column<DateTime>(nullable: false),
                    Description = table.Column<string>(maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JobDomains.Queue", x => x.Name);
                });

            migrationBuilder.CreateTable(
                name: "JobDomains.Server",
                schema: "Hangfire",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    CreatedAt = table.Column<DateTime>(nullable: false),
                    Description = table.Column<string>(maxLength: 200, nullable: true),
                    Name = table.Column<string>(maxLength: 50, nullable: false),
                    PlugPath = table.Column<string>(maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JobDomains.Server", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "JobDomains.ServerPlugin",
                schema: "Hangfire",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    CreatedAt = table.Column<DateTime>(nullable: false),
                    PlugName = table.Column<string>(maxLength: 50, nullable: false),
                    ServerName = table.Column<string>(maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JobDomains.ServerPlugin", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "JobDomains.ServerQueue",
                schema: "Hangfire",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    CreatedAt = table.Column<DateTime>(nullable: false),
                    QueueName = table.Column<string>(maxLength: 50, nullable: false),
                    ServerName = table.Column<string>(maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JobDomains.ServerQueue", x => x.ID);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "JobDomains.Assembly",
                schema: "Hangfire");

            migrationBuilder.DropTable(
                name: "JobDomains.Domain",
                schema: "Hangfire");

            migrationBuilder.DropTable(
                name: "JobDomains.Job",
                schema: "Hangfire");

            migrationBuilder.DropTable(
                name: "JobDomains.JobConstructorParameter",
                schema: "Hangfire");

            migrationBuilder.DropTable(
                name: "JobDomains.Queue",
                schema: "Hangfire");

            migrationBuilder.DropTable(
                name: "JobDomains.Server",
                schema: "Hangfire");

            migrationBuilder.DropTable(
                name: "JobDomains.ServerPlugin",
                schema: "Hangfire");

            migrationBuilder.DropTable(
                name: "JobDomains.ServerQueue",
                schema: "Hangfire");
        }
    }
}
