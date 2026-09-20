using AutoAuctionPlayground.Domain.Entities.Users;
using AutoAuctionPlayground.Domain.Entities.Vehicle;
using AutoAuctionPlayground.Domain.Enums;
using AutoAuctionPlayground.Domain.ValueObjects;

namespace AutoAuctionPlayground.Domain.Entities.Auctions
{
    public class Auction
    {
        private readonly List<AuctionBid> _bids = [];

        public Guid Id { get; private set; }
        public Guid VehicleListingId { get; private set; }

        // Snapshots taken when the auction opens, so PlaceBid can enforce "the seller's company
        // cannot bid" without loading the listing or the seller.
        public Guid SellerUserId { get; private set; }
        public Guid SellerCompanyId { get; private set; }

        public Money StartingPrice { get; private set; } = default!;
        public AuctionStatus Status { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime EndsAt { get; private set; }
        public Money? HighestBidAmount { get; private set; }
        public Guid? HighestBidderUserId { get; private set; }
        public IReadOnlyCollection<AuctionBid> Bids => _bids.AsReadOnly();

        // Optimistic-concurrency token. Persistence maps this as a concurrency token, so every
        // UPDATE becomes "... WHERE id = @id AND version = @versionIRead". Of two racing writers
        // only the first matches a row; the second affects 0 rows, which EF raises as
        // DbUpdateConcurrencyException. The command handler then reloads and calls PlaceBid again
        // (bounded retries), and only a rejection *after* that re-evaluation reaches the user.
        //
        // Example: highest bid is 100,000 at version 24. Two bids arrive at once: 105,000 and 110,000.
        //   110,000 writes first -> highest = 110,000, version 25. The 105,000 request's UPDATE
        //   (WHERE version = 24) affects 0 rows -> reload -> 105,000 > 110,000 is false -> rejected.
        //   That bid was never valid, and this is the first moment the bidder hears about it.
        //   105,000 writes first -> highest = 105,000, version 25. The 110,000 request conflicts,
        //   reloads, 110,000 > 105,000 -> accepted at version 26. Both bids were valid when accepted.
        // Either way the final state is identical; arrival order only decides whether the losing
        // bid is recorded or rejected. Without the token both requests pass the check below against
        // the same stale 100,000 and both get written.
        public long Version { get; private set; }

        private Auction() { }
        private Auction(
            Guid vehicleListingId,
            Guid sellerUserId,
            Guid sellerCompanyId,
            Money startingPrice,
            DateTime createdAtUtc,
            DateTime endsAtUtc)
        {
            Id = Guid.NewGuid();
            VehicleListingId = vehicleListingId;
            SellerUserId = sellerUserId;
            SellerCompanyId = sellerCompanyId;
            StartingPrice = startingPrice;
            Status = AuctionStatus.Open;
            CreatedAt = createdAtUtc;
            EndsAt = endsAtUtc;
            Version = 1;
        }

        // "One open auction per listing" is enforced by a partial unique index in the database
        // (vehicle_listing_id WHERE status = 'Open'), not here: this aggregate cannot see other auctions.
        public static Auction Create(VehicleListing listing, decimal startingPrice, DateTime nowUtc, DateTime endsAtUtc)
        {
            ArgumentNullException.ThrowIfNull(listing);
            listing.EnsurePublished();
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(startingPrice);
            ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(endsAtUtc, nowUtc);

            // The auction is priced in the listing's currency; bids are coerced into it below.
            var price = Money.Of(startingPrice, listing.Details.Price.Currency);
            return new Auction(listing.Id, listing.DealerId, listing.DealerCompanyId, price, nowUtc, endsAtUtc);
        }

        public AuctionBid PlaceBid(User bidder, decimal amount, DateTime nowUtc)
        {
            ArgumentNullException.ThrowIfNull(bidder);
            EnsureOpen();
            EnsureNotEnded(nowUtc);
            EnsureNotSellersCompany(bidder);
            var money = Money.Of(amount, StartingPrice.Currency);
            EnsureBeatsCurrentHighest(money);

            var bid = AuctionBid.Create(Id, bidder.Id, money, nowUtc);
            _bids.Add(bid);
            HighestBidAmount = money;
            HighestBidderUserId = bidder.Id;
            Version++;
            return bid;
        }

        public void Close(DateTime nowUtc)
        {
            EnsureOpen();
            if (nowUtc < EndsAt)
                throw new InvalidOperationException("The auction cannot be closed before it ends.");

            Status = AuctionStatus.Closed;
            Version++;
        }

        public void Cancel()
        {
            EnsureOpen();
            if (_bids.Count > 0)
                throw new InvalidOperationException("An auction with bids cannot be cancelled.");

            Status = AuctionStatus.Cancelled;
            Version++;
        }

        public bool HasWinner => Status == AuctionStatus.Closed && HighestBidderUserId is not null;

        private void EnsureOpen()
        {
            if (Status != AuctionStatus.Open)
                throw new InvalidOperationException($"The auction is {Status}.");
        }

        private void EnsureNotEnded(DateTime nowUtc)
        {
            if (nowUtc >= EndsAt)
                throw new InvalidOperationException("The auction has already ended.");
        }

        private void EnsureNotSellersCompany(User bidder)
        {
            if (bidder.CompanyId == SellerCompanyId)
                throw new InvalidOperationException("The selling company cannot bid on its own auction.");
        }

        // This is the invariant, and under concurrency it is necessary but not sufficient:
        // two requests can both pass it against the same stale HighestBidAmount. Version is what
        // makes it hold.
        private void EnsureBeatsCurrentHighest(Money amount)
        {
            if (HighestBidAmount is null ? amount < StartingPrice : amount <= HighestBidAmount)
                throw new InvalidOperationException(
                    $"Bid must be at least the starting price and above the current highest bid ({HighestBidAmount ?? StartingPrice}).");
        }
    }
}
