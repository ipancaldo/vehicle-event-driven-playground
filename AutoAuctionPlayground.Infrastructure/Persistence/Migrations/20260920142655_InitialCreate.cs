using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutoAuctionPlayground.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "companies",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_companies", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "vehicle_makes",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_vehicle_makes", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    company_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_users", x => x.id);
                    table.ForeignKey(
                        name: "fk_users_companies_company_id",
                        column: x => x.company_id,
                        principalTable: "companies",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "vehicle_models",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    vehicle_make_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    production_start_year = table.Column<int>(type: "integer", nullable: false),
                    production_end_year = table.Column<int>(type: "integer", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_vehicle_models", x => x.id);
                    table.ForeignKey(
                        name: "fk_vehicle_models_vehicle_makes_vehicle_make_id",
                        column: x => x.vehicle_make_id,
                        principalTable: "vehicle_makes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "vehicle_listings",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    dealer_id = table.Column<Guid>(type: "uuid", nullable: false),
                    dealer_company_id = table.Column<Guid>(type: "uuid", nullable: false),
                    vehicle_model_id = table.Column<Guid>(type: "uuid", nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    mileage_km = table.Column<int>(type: "integer", nullable: false),
                    vin = table.Column<string>(type: "character varying(17)", maxLength: 17, nullable: false),
                    year = table.Column<int>(type: "integer", nullable: false),
                    price_amount = table.Column<decimal>(type: "numeric(12,2)", nullable: false),
                    price_currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_vehicle_listings", x => x.id);
                    table.ForeignKey(
                        name: "fk_vehicle_listings_companies_dealer_company_id",
                        column: x => x.dealer_company_id,
                        principalTable: "companies",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_vehicle_listings_users_dealer_id",
                        column: x => x.dealer_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_vehicle_listings_vehicle_models_vehicle_model_id",
                        column: x => x.vehicle_model_id,
                        principalTable: "vehicle_models",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "auctions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    vehicle_listing_id = table.Column<Guid>(type: "uuid", nullable: false),
                    seller_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    seller_company_id = table.Column<Guid>(type: "uuid", nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ends_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    highest_bidder_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    version = table.Column<long>(type: "bigint", nullable: false),
                    highest_bid_amount = table.Column<decimal>(type: "numeric(12,2)", nullable: true),
                    highest_bid_currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: true),
                    starting_price_amount = table.Column<decimal>(type: "numeric(12,2)", nullable: false),
                    starting_price_currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_auctions", x => x.id);
                    table.ForeignKey(
                        name: "fk_auctions_companies_seller_company_id",
                        column: x => x.seller_company_id,
                        principalTable: "companies",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_auctions_users_highest_bidder_user_id",
                        column: x => x.highest_bidder_user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_auctions_users_seller_user_id",
                        column: x => x.seller_user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_auctions_vehicle_listings_vehicle_listing_id",
                        column: x => x.vehicle_listing_id,
                        principalTable: "vehicle_listings",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "listing_price_changes",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    vehicle_listing_id = table.Column<Guid>(type: "uuid", nullable: false),
                    changed_by_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    changed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    new_price_amount = table.Column<decimal>(type: "numeric(12,2)", nullable: false),
                    new_price_currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    old_price_amount = table.Column<decimal>(type: "numeric(12,2)", nullable: false),
                    old_price_currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_listing_price_changes", x => x.id);
                    table.ForeignKey(
                        name: "fk_listing_price_changes_vehicle_listings_vehicle_listing_id",
                        column: x => x.vehicle_listing_id,
                        principalTable: "vehicle_listings",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "auction_bids",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    auction_id = table.Column<Guid>(type: "uuid", nullable: false),
                    bidder_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    placed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    amount_amount = table.Column<decimal>(type: "numeric(12,2)", nullable: false),
                    amount_currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_auction_bids", x => x.id);
                    table.ForeignKey(
                        name: "fk_auction_bids_auctions_auction_id",
                        column: x => x.auction_id,
                        principalTable: "auctions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_auction_bids_users_bidder_user_id",
                        column: x => x.bidder_user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "vehicle_transactions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    vehicle_listing_id = table.Column<Guid>(type: "uuid", nullable: false),
                    auction_id = table.Column<Guid>(type: "uuid", nullable: true),
                    buyer_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    kind = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    finalized_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    final_price_amount = table.Column<decimal>(type: "numeric(12,2)", nullable: false),
                    final_price_currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_vehicle_transactions", x => x.id);
                    table.ForeignKey(
                        name: "fk_vehicle_transactions_auctions_auction_id",
                        column: x => x.auction_id,
                        principalTable: "auctions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_vehicle_transactions_users_buyer_user_id",
                        column: x => x.buyer_user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_vehicle_transactions_vehicle_listings_vehicle_listing_id",
                        column: x => x.vehicle_listing_id,
                        principalTable: "vehicle_listings",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_auction_bids_auction_id_placed_at",
                table: "auction_bids",
                columns: new[] { "auction_id", "placed_at" });

            migrationBuilder.CreateIndex(
                name: "ix_auction_bids_bidder_user_id",
                table: "auction_bids",
                column: "bidder_user_id");

            migrationBuilder.CreateIndex(
                name: "ix_auctions_highest_bidder_user_id",
                table: "auctions",
                column: "highest_bidder_user_id");

            migrationBuilder.CreateIndex(
                name: "ix_auctions_one_open_per_listing",
                table: "auctions",
                column: "vehicle_listing_id",
                unique: true,
                filter: "status = 'Open'");

            migrationBuilder.CreateIndex(
                name: "ix_auctions_seller_company_id",
                table: "auctions",
                column: "seller_company_id");

            migrationBuilder.CreateIndex(
                name: "ix_auctions_seller_user_id",
                table: "auctions",
                column: "seller_user_id");

            migrationBuilder.CreateIndex(
                name: "ix_auctions_status_ends_at",
                table: "auctions",
                columns: new[] { "status", "ends_at" });

            migrationBuilder.CreateIndex(
                name: "ix_companies_name",
                table: "companies",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_listing_price_changes_vehicle_listing_id_changed_at",
                table: "listing_price_changes",
                columns: new[] { "vehicle_listing_id", "changed_at" });

            migrationBuilder.CreateIndex(
                name: "ix_users_company_id",
                table: "users",
                column: "company_id");

            migrationBuilder.CreateIndex(
                name: "ix_vehicle_listings_dealer_company_id",
                table: "vehicle_listings",
                column: "dealer_company_id");

            migrationBuilder.CreateIndex(
                name: "ix_vehicle_listings_dealer_id",
                table: "vehicle_listings",
                column: "dealer_id");

            migrationBuilder.CreateIndex(
                name: "ix_vehicle_listings_status",
                table: "vehicle_listings",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "ix_vehicle_listings_vehicle_model_id",
                table: "vehicle_listings",
                column: "vehicle_model_id");

            migrationBuilder.CreateIndex(
                name: "ix_vehicle_makes_name",
                table: "vehicle_makes",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_vehicle_models_vehicle_make_id_name",
                table: "vehicle_models",
                columns: new[] { "vehicle_make_id", "name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_vehicle_transactions_buyer_user_id",
                table: "vehicle_transactions",
                column: "buyer_user_id");

            migrationBuilder.CreateIndex(
                name: "ix_vehicle_transactions_one_per_auction",
                table: "vehicle_transactions",
                column: "auction_id",
                unique: true,
                filter: "auction_id IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_vehicle_transactions_one_per_listing",
                table: "vehicle_transactions",
                column: "vehicle_listing_id",
                unique: true);

            // VIN unique per company while the listing is active (Draft/Published/Paused). Declared
            // as raw SQL because EF cannot express a composite index over an owner column
            // (dealer_company_id) and a complex-type column (vin). See VehicleListingConfiguration.
            migrationBuilder.Sql("""
                CREATE UNIQUE INDEX ix_vehicle_listings_active_vin_per_company
                    ON vehicle_listings (dealer_company_id, vin)
                    WHERE status IN ('Draft', 'Published', 'Paused');
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP INDEX IF EXISTS ix_vehicle_listings_active_vin_per_company;");

            migrationBuilder.DropTable(
                name: "auction_bids");

            migrationBuilder.DropTable(
                name: "listing_price_changes");

            migrationBuilder.DropTable(
                name: "vehicle_transactions");

            migrationBuilder.DropTable(
                name: "auctions");

            migrationBuilder.DropTable(
                name: "vehicle_listings");

            migrationBuilder.DropTable(
                name: "users");

            migrationBuilder.DropTable(
                name: "vehicle_models");

            migrationBuilder.DropTable(
                name: "companies");

            migrationBuilder.DropTable(
                name: "vehicle_makes");
        }
    }
}
