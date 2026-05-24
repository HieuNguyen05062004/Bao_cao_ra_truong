using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Core.Shared.Migrations
{
    /// <inheritdoc />
    public partial class FixCascadeDeleteBorrowDetail : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BorrowDetails_BorrowTickets_TicketID",
                table: "BorrowDetails");

            migrationBuilder.AddForeignKey(
                name: "FK_BorrowDetails_BorrowTickets_TicketID",
                table: "BorrowDetails",
                column: "TicketID",
                principalTable: "BorrowTickets",
                principalColumn: "TicketID",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BorrowDetails_BorrowTickets_TicketID",
                table: "BorrowDetails");

            migrationBuilder.AddForeignKey(
                name: "FK_BorrowDetails_BorrowTickets_TicketID",
                table: "BorrowDetails",
                column: "TicketID",
                principalTable: "BorrowTickets",
                principalColumn: "TicketID");
        }
    }
}
