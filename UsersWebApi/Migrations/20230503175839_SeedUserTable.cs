using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UsersWebApi.Migrations
{
    /// <inheritdoc />
    public partial class SeedUserTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "Admin", "Birthday", "CreatedBy", "CreatedOn", "Gender", "Login", "ModifiedBy", "ModifiedOn", "Name", "Password", "RevokedBy", "RevokedOn" },
                values: new object[] { new Guid("2476081d-c09b-4c24-bc68-584ee28fb87e"), true, new DateTime(2001, 8, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new DateTime(2023, 5, 3, 20, 58, 39, 81, DateTimeKind.Local).AddTicks(9137), 1, "Admin", "", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Admin", "123", "", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("2476081d-c09b-4c24-bc68-584ee28fb87e"));
        }
    }
}
