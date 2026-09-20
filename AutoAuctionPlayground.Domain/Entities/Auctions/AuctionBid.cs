using AutoAuctionPlayground.Domain.ValueObjects;

namespace AutoAuctionPlayground.Domain.Entities.Auctions
{
    // Append-only history. Whether a bid is "the current one" is not stored here: the Auction
    // root holds HighestBidAmount/HighestBidderUserId, and losing bids are rejected rather than
    // recorded, so the latest accepted bid is always the highest.
    public class AuctionBid
    {
        public Guid Id { get; private set; }
        public Guid AuctionId { get; private set; }
        public Guid BidderUserId { get; private set; }
        public Money Amount { get; private set; } = default!;
        public DateTime PlacedAt { get; private set; }

        private AuctionBid() { }
        private AuctionBid(Guid auctionId, Guid bidderUserId, Money amount, DateTime placedAtUtc)
        {
            Id = Guid.NewGuid();
            AuctionId = auctionId;
            BidderUserId = bidderUserId;
            Amount = amount;
            PlacedAt = placedAtUtc;
        }

        internal static AuctionBid Create(Guid auctionId, Guid bidderUserId, Money amount, DateTime placedAtUtc)
            => new(auctionId, bidderUserId, amount, placedAtUtc);
    }
}
