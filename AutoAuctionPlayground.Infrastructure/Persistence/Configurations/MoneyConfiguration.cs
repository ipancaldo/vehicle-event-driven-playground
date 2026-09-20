using AutoAuctionPlayground.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AutoAuctionPlayground.Infrastructure.Persistence.Configurations
{
    internal static class MoneyConfiguration
    {
        // Money has no identity, so it is mapped as a complex type, not an owned entity type:
        // two plain columns (<prefix>_amount, <prefix>_currency) on the owner's own row, with no
        // shadow key and no separate node in the change tracker's entity graph. OwnsOne was tried
        // first and produced a real EF Core bug when the same CLR type (Money) was owned both by
        // an aggregate root and, independently, by a child in that root's collection (Auction owns
        // HighestBidAmount; each AuctionBid owns Amount) — EF's fixup crashed with
        // "The property 'AuctionId' belongs to the type 'Auction.HighestBidAmount#Money', but is
        // being used with an instance of type 'AuctionBid.Amount#Money'". Complex types don't have
        // shadow keys at all, so that class of collision cannot happen.
        public static void ConfigureMoney(this ComplexPropertyBuilder<Money> money, string prefix)
        {
            money.Property(m => m.Amount)
                 .HasColumnName($"{prefix}_amount")
                 .HasColumnType("numeric(12,2)");

            money.Property(m => m.Currency)
                 .HasColumnName($"{prefix}_currency")
                 .HasConversion<string>()
                 .HasMaxLength(3);
        }
    }
}
