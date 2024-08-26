using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace sociosphere.Migrations
{
    /// <inheritdoc />
    public partial class m1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "addcomplaints",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    flatno = table.Column<int>(type: "int", nullable: false),
                    WriteComplaint = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    complaintstatus = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    raisedate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    resolvedate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_addcomplaints", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "addflats",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    flatno = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    floorno = table.Column<int>(type: "int", nullable: false),
                    wingname = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    flattype = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_addflats", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "alloteflats",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    flatno = table.Column<int>(type: "int", nullable: false),
                    floorno = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    wingname = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    flattype = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    moveindate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    allotdate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_alloteflats", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "announcements",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Announcement = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    AnnounceDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_announcements", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "billmanagements",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    WingName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FlatNo = table.Column<int>(type: "int", nullable: false),
                    AmountPay = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Month = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PaidStatus = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BillReleaseDt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    BillSbmtDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_billmanagements", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "gatemanagements",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WingName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FlatNo = table.Column<int>(type: "int", nullable: false),
                    VisitorName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Address = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    InDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    OutDateTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Action = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_gatemanagements", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "userregs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    photo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    city = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    role = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    password = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_userregs", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "addcomplaints");

            migrationBuilder.DropTable(
                name: "addflats");

            migrationBuilder.DropTable(
                name: "alloteflats");

            migrationBuilder.DropTable(
                name: "announcements");

            migrationBuilder.DropTable(
                name: "billmanagements");

            migrationBuilder.DropTable(
                name: "gatemanagements");

            migrationBuilder.DropTable(
                name: "userregs");
        }
    }
}
