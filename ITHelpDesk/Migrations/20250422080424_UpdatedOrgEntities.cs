using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ITHelpDesk.Migrations
{
    /// <inheritdoc />
    public partial class UpdatedOrgEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Departments_AspNetUsers_DepartmentManagerId",
                table: "Departments");

            migrationBuilder.DropForeignKey(
                name: "FK_SubDepartments_AspNetUsers_SubDepartmentManagerId",
                table: "SubDepartments");

            migrationBuilder.DropForeignKey(
                name: "FK_SubDepartments_Departments_DepartmentId",
                table: "SubDepartments");

            migrationBuilder.AddForeignKey(
                name: "FK_Departments_AspNetUsers_DepartmentManagerId",
                table: "Departments",
                column: "DepartmentManagerId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_SubDepartments_AspNetUsers_SubDepartmentManagerId",
                table: "SubDepartments",
                column: "SubDepartmentManagerId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_SubDepartments_Departments_DepartmentId",
                table: "SubDepartments",
                column: "DepartmentId",
                principalTable: "Departments",
                principalColumn: "DepartmentId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Departments_AspNetUsers_DepartmentManagerId",
                table: "Departments");

            migrationBuilder.DropForeignKey(
                name: "FK_SubDepartments_AspNetUsers_SubDepartmentManagerId",
                table: "SubDepartments");

            migrationBuilder.DropForeignKey(
                name: "FK_SubDepartments_Departments_DepartmentId",
                table: "SubDepartments");

            migrationBuilder.AddForeignKey(
                name: "FK_Departments_AspNetUsers_DepartmentManagerId",
                table: "Departments",
                column: "DepartmentManagerId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SubDepartments_AspNetUsers_SubDepartmentManagerId",
                table: "SubDepartments",
                column: "SubDepartmentManagerId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SubDepartments_Departments_DepartmentId",
                table: "SubDepartments",
                column: "DepartmentId",
                principalTable: "Departments",
                principalColumn: "DepartmentId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
