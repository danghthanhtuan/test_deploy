using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApp.Migrations
{
    /// <inheritdoc />
    public partial class AddServiceTypeAndContractNumberToCompany : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Company",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    customerID = table.Column<string>(type: "char(15)", unicode: false, fixedLength: true, maxLength: 15, nullable: false),
                    companyName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    taxCode = table.Column<string>(type: "varchar(30)", unicode: false, maxLength: 30, nullable: false),
                    companyAccount = table.Column<string>(type: "varchar(40)", unicode: false, maxLength: 40, nullable: false),
                    accountIssuedDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    cPhoneNumber = table.Column<string>(type: "varchar(10)", unicode: false, maxLength: 10, nullable: false),
                    cAddress = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    customerType = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Company__3213E83F6A5D0E7A", x => x.id);
                    table.UniqueConstraint("AK_Company_customerID", x => x.customerID);
                });

            migrationBuilder.CreateTable(
                name: "STAFF",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    staffID = table.Column<string>(type: "char(10)", unicode: false, fixedLength: true, maxLength: 10, nullable: false),
                    staffNAME = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    staffPhone = table.Column<string>(type: "varchar(10)", unicode: false, maxLength: 10, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__STAFF__3213E83F5B137B3C", x => x.id);
                    table.UniqueConstraint("AK_STAFF_staffID", x => x.staffID);
                });

            migrationBuilder.CreateTable(
                name: "Account",
                columns: table => new
                {
                    customerID = table.Column<string>(type: "char(15)", unicode: false, fixedLength: true, maxLength: 15, nullable: false),
                    rootAccount = table.Column<string>(type: "varchar(40)", unicode: false, maxLength: 40, nullable: false),
                    rootName = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    rPhoneNumber = table.Column<string>(type: "varchar(10)", unicode: false, maxLength: 10, nullable: false),
                    operatingStatus = table.Column<bool>(type: "bit", nullable: false),
                    dateOfBirth = table.Column<DateTime>(type: "datetime", nullable: false),
                    gender = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Account__B611CB9D4A9754C0", x => x.customerID);
                    table.ForeignKey(
                        name: "FK_Account_Company",
                        column: x => x.customerID,
                        principalTable: "Company",
                        principalColumn: "customerID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LOGINclient",
                columns: table => new
                {
                    customerID = table.Column<string>(type: "char(15)", unicode: false, fixedLength: true, maxLength: 15, nullable: false),
                    userName = table.Column<string>(type: "varchar(40)", unicode: false, maxLength: 40, nullable: false),
                    passWordclient = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__LOGINcli__B611CB9D8E610CB8", x => x.customerID);
                    table.ForeignKey(
                        name: "FK__LOGINclie__custo__44FF419A",
                        column: x => x.customerID,
                        principalTable: "Company",
                        principalColumn: "customerID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LOGINadmin",
                columns: table => new
                {
                    staffID = table.Column<string>(type: "char(10)", unicode: false, fixedLength: true, maxLength: 10, nullable: false),
                    userNameAD = table.Column<string>(type: "varchar(40)", unicode: false, maxLength: 40, nullable: false),
                    passWordAD = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__LOGINadm__6465E19E8284D9B9", x => x.staffID);
                    table.ForeignKey(
                        name: "FK__LOGINadmi__staff__4BAC3F29",
                        column: x => x.staffID,
                        principalTable: "STAFF",
                        principalColumn: "staffID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "UQ__Account__53AA789A584863BE",
                table: "Account",
                column: "rPhoneNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ__Account__54E43460B677250D",
                table: "Account",
                column: "rootAccount",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ__Company__978110643401CA0C",
                table: "Company",
                column: "cPhoneNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ__Company__A80FF6F89EC30976",
                table: "Company",
                column: "companyAccount",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ__Company__B611CB9CAA8B2E97",
                table: "Company",
                column: "customerID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ__Company__D97858A6F98D8988",
                table: "Company",
                column: "taxCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ__STAFF__61496E384EC44037",
                table: "STAFF",
                column: "staffPhone",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ__STAFF__6465E19FC3EC9EF7",
                table: "STAFF",
                column: "staffID",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Account");

            migrationBuilder.DropTable(
                name: "LOGINadmin");

            migrationBuilder.DropTable(
                name: "LOGINclient");

            migrationBuilder.DropTable(
                name: "STAFF");

            migrationBuilder.DropTable(
                name: "Company");
        }
    }
}
