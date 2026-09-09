using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OkVolleyVibes.Infrastructure.Persistence.Migrations
{
    /// <summary>
    /// Bootstrap data: the five roles and the three founding club accounts (Stefan, Luka, Aleksa)
    /// with their role assignments and player profiles.
    ///
    /// The password hash is a fixed hash of a DEVELOPMENT password ("DevPassw0rd!42"). This
    /// migration is a dev/staging bootstrap — do NOT apply it against a real production database;
    /// production accounts must be provisioned with per-user passwords or a reset flow.
    /// </summary>
    public partial class SeedBootstrapAccounts : Migration
    {
        // Fixed identifiers so the rows are stable and removable.
        private static readonly Guid CeoRole = new("a1a1a1a1-0000-0000-0000-000000000001");
        private static readonly Guid FinanceManagerRole = new("a1a1a1a1-0000-0000-0000-000000000002");
        private static readonly Guid CoachRole = new("a1a1a1a1-0000-0000-0000-000000000003");
        private static readonly Guid RecreationCoordinatorRole = new("a1a1a1a1-0000-0000-0000-000000000004");
        private static readonly Guid PlayerRole = new("a1a1a1a1-0000-0000-0000-000000000005");

        private static readonly Guid StefanUser = new("b2b2b2b2-0000-0000-0000-000000000001");
        private static readonly Guid LukaUser = new("b2b2b2b2-0000-0000-0000-000000000002");
        private static readonly Guid AleksaUser = new("b2b2b2b2-0000-0000-0000-000000000003");

        private static readonly Guid StefanProfile = new("c3c3c3c3-0000-0000-0000-000000000001");
        private static readonly Guid LukaProfile = new("c3c3c3c3-0000-0000-0000-000000000002");
        private static readonly Guid AleksaProfile = new("c3c3c3c3-0000-0000-0000-000000000003");

        // Identity PBKDF2 hash of the development password "DevPassw0rd!42".
        private const string DevPasswordHash =
            "AQAAAAIAAYagAAAAEFa1Q0gcGm1GtrPJi23adV35WOqCsce+md0p2Mlmf/YNi4nhgVJHk6tFSqj06sOk3A==";

        private static readonly DateTime SeededAtUtc = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "Name", "NormalizedName", "ConcurrencyStamp" },
                values: new object[,]
                {
                    { CeoRole, "CEO", "CEO", "a1a1a1a1-1111-1111-1111-000000000001" },
                    { FinanceManagerRole, "FinanceManager", "FINANCEMANAGER", "a1a1a1a1-1111-1111-1111-000000000002" },
                    { CoachRole, "Coach", "COACH", "a1a1a1a1-1111-1111-1111-000000000003" },
                    { RecreationCoordinatorRole, "RecreationCoordinator", "RECREATIONCOORDINATOR", "a1a1a1a1-1111-1111-1111-000000000004" },
                    { PlayerRole, "Player", "PLAYER", "a1a1a1a1-1111-1111-1111-000000000005" },
                });

            migrationBuilder.InsertData(
                table: "AspNetUsers",
                columns: new[]
                {
                    "Id", "FirstName", "LastName", "PreferredLanguage", "CreatedAtUtc",
                    "UserName", "NormalizedUserName", "Email", "NormalizedEmail", "EmailConfirmed",
                    "PasswordHash", "SecurityStamp", "ConcurrencyStamp",
                    "PhoneNumberConfirmed", "TwoFactorEnabled", "LockoutEnabled", "AccessFailedCount",
                },
                values: new object[,]
                {
                    {
                        StefanUser, "Stefan", "Ivkovic", "en", SeededAtUtc,
                        "ivkovics@outlook.com", "IVKOVICS@OUTLOOK.COM", "ivkovics@outlook.com", "IVKOVICS@OUTLOOK.COM", true,
                        DevPasswordHash, "b2b2b2b2-1111-1111-1111-000000000001", "b2b2b2b2-2222-2222-2222-000000000001",
                        false, false, true, 0,
                    },
                    {
                        LukaUser, "Luka", "Lukic", "en", SeededAtUtc,
                        "luka9295@gmail.com", "LUKA9295@GMAIL.COM", "luka9295@gmail.com", "LUKA9295@GMAIL.COM", true,
                        DevPasswordHash, "b2b2b2b2-1111-1111-1111-000000000002", "b2b2b2b2-2222-2222-2222-000000000002",
                        false, false, true, 0,
                    },
                    {
                        AleksaUser, "Aleksa", "Pajic", "en", SeededAtUtc,
                        "a.pajic.98@gmail.com", "A.PAJIC.98@GMAIL.COM", "a.pajic.98@gmail.com", "A.PAJIC.98@GMAIL.COM", true,
                        DevPasswordHash, "b2b2b2b2-1111-1111-1111-000000000003", "b2b2b2b2-2222-2222-2222-000000000003",
                        false, false, true, 0,
                    },
                });

            migrationBuilder.InsertData(
                table: "AspNetUserRoles",
                columns: new[] { "UserId", "RoleId" },
                values: new object[,]
                {
                    { StefanUser, CeoRole },
                    { StefanUser, CoachRole },
                    { StefanUser, RecreationCoordinatorRole },
                    { StefanUser, PlayerRole },
                    { LukaUser, CoachRole },
                    { LukaUser, PlayerRole },
                    { AleksaUser, RecreationCoordinatorRole },
                    { AleksaUser, PlayerRole },
                });

            migrationBuilder.InsertData(
                table: "PlayerProfiles",
                columns: new[] { "Id", "UserId", "CreatedAtUtc", "MembershipStatus" },
                values: new object[,]
                {
                    { StefanProfile, StefanUser, SeededAtUtc, "Pending" },
                    { LukaProfile, LukaUser, SeededAtUtc, "Pending" },
                    { AleksaProfile, AleksaUser, SeededAtUtc, "Pending" },
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "PlayerProfiles",
                keyColumn: "Id",
                keyValues: new object[] { StefanProfile, LukaProfile, AleksaProfile });

            migrationBuilder.DeleteData(
                table: "AspNetUserRoles",
                keyColumns: new[] { "UserId", "RoleId" },
                keyValues: new object[,]
                {
                    { StefanUser, CeoRole },
                    { StefanUser, CoachRole },
                    { StefanUser, RecreationCoordinatorRole },
                    { StefanUser, PlayerRole },
                    { LukaUser, CoachRole },
                    { LukaUser, PlayerRole },
                    { AleksaUser, RecreationCoordinatorRole },
                    { AleksaUser, PlayerRole },
                });

            migrationBuilder.DeleteData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValues: new object[] { StefanUser, LukaUser, AleksaUser });

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValues: new object[]
                {
                    CeoRole, FinanceManagerRole, CoachRole, RecreationCoordinatorRole, PlayerRole,
                });
        }
    }
}
