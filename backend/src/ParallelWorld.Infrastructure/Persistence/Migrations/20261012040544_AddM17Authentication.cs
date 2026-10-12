using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ParallelWorld.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddM17Authentication : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ux_device_installations_public_id",
                table: "device_installations");

            migrationBuilder.CreateTable(
                name: "account_recovery_codes",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    code_hash = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    consumed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    revoked_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    version = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_account_recovery_codes", x => x.id);
                    table.ForeignKey(
                        name: "fk_account_recovery_codes_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.AddCheckConstraint(
                name: "ck_users_account_credentials",
                table: "users",
                sql: "(account_type = 'guest' AND email IS NULL AND normalized_email IS NULL AND password_hash IS NULL) OR (account_type = 'registered' AND email IS NOT NULL AND normalized_email IS NOT NULL AND password_hash IS NOT NULL)");

            migrationBuilder.CreateIndex(
                name: "ux_device_installations_user_public_id",
                table: "device_installations",
                columns: new[] { "user_id", "installation_public_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ux_account_recovery_codes_active_user",
                table: "account_recovery_codes",
                column: "user_id",
                unique: true,
                filter: "consumed_at IS NULL AND revoked_at IS NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "account_recovery_codes");

            migrationBuilder.DropCheckConstraint(
                name: "ck_users_account_credentials",
                table: "users");

            migrationBuilder.DropIndex(
                name: "ux_device_installations_user_public_id",
                table: "device_installations");

            migrationBuilder.CreateIndex(
                name: "ux_device_installations_public_id",
                table: "device_installations",
                column: "installation_public_id",
                unique: true);
        }
    }
}
