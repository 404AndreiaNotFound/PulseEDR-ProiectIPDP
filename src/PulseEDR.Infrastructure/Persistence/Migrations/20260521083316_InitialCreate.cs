using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PulseEDR.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "cves",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CveId = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    ProductName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    MaxAffectedVersionExclusive = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    FixedVersion = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Severity = table.Column<int>(type: "integer", nullable: false),
                    CvssScore = table.Column<double>(type: "double precision", nullable: false),
                    Description = table.Column<string>(type: "character varying(4096)", maxLength: 4096, nullable: false),
                    Remediation = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    ReferenceUrl = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    PublishedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_cves", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "scans",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MachineName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    OsVersion = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    risk_score_value = table.Column<int>(type: "integer", nullable: true),
                    risk_score_severity = table.Column<int>(type: "integer", nullable: true),
                    StartedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_scans", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "alerts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ScanResultId = table.Column<Guid>(type: "uuid", nullable: false),
                    Category = table.Column<int>(type: "integer", nullable: false),
                    Severity = table.Column<int>(type: "integer", nullable: false),
                    Score = table.Column<int>(type: "integer", nullable: false),
                    Title = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Description = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    Remediation = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    Evidence = table.Column<string>(type: "jsonb", nullable: false),
                    CveId = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_alerts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_alerts_scans_ScanResultId",
                        column: x => x.ScanResultId,
                        principalTable: "scans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "connections",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ScanResultId = table.Column<Guid>(type: "uuid", nullable: false),
                    LocalAddress = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    LocalPort = table.Column<int>(type: "integer", nullable: false),
                    RemoteAddress = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    RemotePort = table.Column<int>(type: "integer", nullable: false),
                    Protocol = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: false),
                    State = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    OwnerPid = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_connections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_connections_scans_ScanResultId",
                        column: x => x.ScanResultId,
                        principalTable: "scans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "processes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ScanResultId = table.Column<Guid>(type: "uuid", nullable: false),
                    Pid = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    ExecutablePath = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    CommandLine = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    ParentPid = table.Column<int>(type: "integer", nullable: true),
                    StartTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_processes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_processes_scans_ScanResultId",
                        column: x => x.ScanResultId,
                        principalTable: "scans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "recent_files",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ScanResultId = table.Column<Guid>(type: "uuid", nullable: false),
                    FullPath = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    FileName = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    Extension = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    SizeBytes = table.Column<long>(type: "bigint", nullable: false),
                    LastModified = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Sha256 = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_recent_files", x => x.Id);
                    table.ForeignKey(
                        name: "FK_recent_files_scans_ScanResultId",
                        column: x => x.ScanResultId,
                        principalTable: "scans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "software",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ScanResultId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    Version = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Publisher = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    InstallLocation = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    InstallDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_software", x => x.Id);
                    table.ForeignKey(
                        name: "FK_software_scans_ScanResultId",
                        column: x => x.ScanResultId,
                        principalTable: "scans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_alerts_Category",
                table: "alerts",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_alerts_ScanResultId",
                table: "alerts",
                column: "ScanResultId");

            migrationBuilder.CreateIndex(
                name: "IX_alerts_Severity",
                table: "alerts",
                column: "Severity");

            migrationBuilder.CreateIndex(
                name: "IX_connections_RemoteAddress",
                table: "connections",
                column: "RemoteAddress");

            migrationBuilder.CreateIndex(
                name: "IX_connections_ScanResultId",
                table: "connections",
                column: "ScanResultId");

            migrationBuilder.CreateIndex(
                name: "IX_cves_CveId",
                table: "cves",
                column: "CveId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_cves_ProductName",
                table: "cves",
                column: "ProductName");

            migrationBuilder.CreateIndex(
                name: "IX_processes_Name",
                table: "processes",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_processes_ScanResultId",
                table: "processes",
                column: "ScanResultId");

            migrationBuilder.CreateIndex(
                name: "IX_recent_files_Extension",
                table: "recent_files",
                column: "Extension");

            migrationBuilder.CreateIndex(
                name: "IX_recent_files_ScanResultId",
                table: "recent_files",
                column: "ScanResultId");

            migrationBuilder.CreateIndex(
                name: "IX_scans_StartedAt",
                table: "scans",
                column: "StartedAt");

            migrationBuilder.CreateIndex(
                name: "IX_scans_Status",
                table: "scans",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_software_Name",
                table: "software",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_software_ScanResultId",
                table: "software",
                column: "ScanResultId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "alerts");

            migrationBuilder.DropTable(
                name: "connections");

            migrationBuilder.DropTable(
                name: "cves");

            migrationBuilder.DropTable(
                name: "processes");

            migrationBuilder.DropTable(
                name: "recent_files");

            migrationBuilder.DropTable(
                name: "software");

            migrationBuilder.DropTable(
                name: "scans");
        }
    }
}
