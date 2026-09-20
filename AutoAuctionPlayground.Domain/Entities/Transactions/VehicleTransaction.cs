using AutoAuctionPlayground.Domain.Entities.Auctions;
using AutoAuctionPlayground.Domain.Entities.Users;
using AutoAuctionPlayground.Domain.Entities.Vehicle;
using AutoAuctionPlayground.Domain.Enums;
using AutoAuctionPlayground.Domain.ValueObjects;

namespace AutoAuctionPlayground.Domain.Entities.Transactions
{
    // The record of a completed sale. A transaction is always about a listing; AuctionId only says
    // *how* it sold, which avoids the "exactly one of two nullable FKs" shape.
    //
    // Concurrency hint (double sale): two buyers can both see an Active listing and both pass the
    // checks below. The guarantee that only one transaction exists per listing is a UNIQUE index on
    // vehicle_listing_id in the database; the loser gets a constraint violation that the application
    // maps to a conflict. Marking the listing Sold happens in the same database transaction as this
    // insert (application layer), never as a separate step.
    public class VehicleTransaction
    {
        public Guid Id { get; private set; }
        public Guid VehicleListingId { get; private set; }
        public Guid? AuctionId { get; private set; }
        public Guid BuyerUserId { get; private set; }
        public Money FinalPrice { get; private set; } = default!;
        public VehicleTransactionKind Kind { get; private set; }
        public DateTime FinalizedAt { get; private set; }

        private VehicleTransaction() { }
        private VehicleTransaction(
            Guid vehicleListingId,
            Guid? auctionId,
            Guid buyerUserId,
            Money finalPrice,
            VehicleTransactionKind kind,
            DateTime finalizedAtUtc)
        {
            Id = Guid.NewGuid();
            VehicleListingId = vehicleListingId;
            AuctionId = auctionId;
            BuyerUserId = buyerUserId;
            FinalPrice = finalPrice;
            Kind = kind;
            FinalizedAt = finalizedAtUtc;
        }

        // Whether the listing is currently under an open auction is an application-level check
        // (query for an Open auction on this listing); this aggregate cannot see auctions.
        public static VehicleTransaction CreateDirectSale(VehicleListing listing, User buyer, DateTime nowUtc)
        {
            ArgumentNullException.ThrowIfNull(listing);
            ArgumentNullException.ThrowIfNull(buyer);
            listing.EnsurePublished();
            EnsureNotSellersCompany(listing, buyer);

            return new VehicleTransaction(
                listing.Id,
                auctionId: null,
                buyer.Id,
                listing.Details.Price,
                VehicleTransactionKind.DirectSale,
                nowUtc);
        }

        public static VehicleTransaction CreateFromAuction(Auction auction, DateTime nowUtc)
        {
            ArgumentNullException.ThrowIfNull(auction);
            EnsureHasWinner(auction);

            return new VehicleTransaction(
                auction.VehicleListingId,
                auction.Id,
                auction.HighestBidderUserId!.Value,
                auction.HighestBidAmount!,
                VehicleTransactionKind.AuctionWin,
                nowUtc);
        }

        private static void EnsureNotSellersCompany(VehicleListing listing, User buyer)
        {
            if (buyer.CompanyId == listing.DealerCompanyId)
                throw new InvalidOperationException("A company cannot buy its own listing.");
        }

        private static void EnsureHasWinner(Auction auction)
        {
            if (!auction.HasWinner)
                throw new InvalidOperationException("Only a closed auction with a winning bid can produce a transaction.");
        }
    }
}
