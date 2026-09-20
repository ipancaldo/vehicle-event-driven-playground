using AutoAuctionPlayground.Domain.Entities.Auctions;
using AutoAuctionPlayground.Domain.Entities.Companies;
using AutoAuctionPlayground.Domain.Entities.Users;
using AutoAuctionPlayground.Domain.Entities.Vehicle;
using AutoAuctionPlayground.Domain.Enums;

namespace AutoAuctionPlayground.Tests.Domain
{
    public class AuctionTests
    {
        private static readonly DateTime Now = new(2026, 9, 20, 12, 0, 0, DateTimeKind.Utc);
        private static readonly DateTime EndsAt = Now.AddDays(3);

        private static User CreateUser(string company = "Some Dealer") =>
            User.Create(Company.Create(company), "Someone");

        private static VehicleListing CreatePublishedListing(User dealer)
        {
            var model = VehicleMake.Create("Ford").AddModel("Focus", 1998);
            var listing = VehicleListing.Create(dealer, "ABC123", model, 2017, 127822, 42563);
            listing.Publish();
            return listing;
        }

        private static Auction CreateOpenAuction(out User seller)
        {
            seller = CreateUser("Seller Co");
            return Auction.Create(CreatePublishedListing(seller), startingPrice: 40000, Now, EndsAt);
        }

        [Fact]
        public void Create_FromPublishedListing_OpensWithSellerSnapshot()
        {
            var seller = CreateUser("Seller Co");
            var listing = CreatePublishedListing(seller);

            var auction = Auction.Create(listing, 40000, Now, EndsAt);

            Assert.Equal(AuctionStatus.Open, auction.Status);
            Assert.Equal(listing.Id, auction.VehicleListingId);
            Assert.Equal(seller.Id, auction.SellerUserId);
            Assert.Equal(seller.CompanyId, auction.SellerCompanyId);
            Assert.Null(auction.HighestBidAmount);
            Assert.Null(auction.HighestBidderUserId);
            Assert.Equal(1, auction.Version);
            Assert.Empty(auction.Bids);
        }

        [Fact]
        public void Create_FromDraftListing_Throws()
        {
            var model = VehicleMake.Create("Ford").AddModel("Focus", 1998);
            var draft = VehicleListing.Create(CreateUser(), "ABC123", model, 2017, 127822, 42563);

            Assert.Throws<InvalidOperationException>(() => Auction.Create(draft, 40000, Now, EndsAt));
        }

        [Fact]
        public void Create_WithNonPositiveStartingPrice_Throws()
        {
            var listing = CreatePublishedListing(CreateUser());

            Assert.Throws<ArgumentOutOfRangeException>(() => Auction.Create(listing, 0, Now, EndsAt));
        }

        [Fact]
        public void Create_EndingInThePast_Throws()
        {
            var listing = CreatePublishedListing(CreateUser());

            Assert.Throws<ArgumentOutOfRangeException>(() => Auction.Create(listing, 40000, Now, Now));
        }

        [Fact]
        public void PlaceBid_FirstBidAtStartingPrice_BecomesHighestAndBumpsVersion()
        {
            var auction = CreateOpenAuction(out _);
            var bidder = CreateUser("Buyer Co");

            var bid = auction.PlaceBid(bidder, 40000, Now.AddHours(1));

            Assert.Equal(40000, auction.HighestBidAmount!.Amount);
            Assert.Equal(bidder.Id, auction.HighestBidderUserId);
            Assert.Equal(2, auction.Version);
            Assert.Same(bid, Assert.Single(auction.Bids));
            Assert.Equal(auction.Id, bid.AuctionId);
        }

        [Fact]
        public void PlaceBid_BelowStartingPrice_Throws()
        {
            var auction = CreateOpenAuction(out _);

            Assert.Throws<InvalidOperationException>(() => auction.PlaceBid(CreateUser("Buyer Co"), 39999, Now));
        }

        [Fact]
        public void PlaceBid_HigherThanCurrent_ReplacesHighest()
        {
            var auction = CreateOpenAuction(out _);
            var first = CreateUser("Buyer A");
            var second = CreateUser("Buyer B");
            auction.PlaceBid(first, 40000, Now);

            auction.PlaceBid(second, 41000, Now.AddMinutes(1));

            Assert.Equal(41000, auction.HighestBidAmount!.Amount);
            Assert.Equal(second.Id, auction.HighestBidderUserId);
            Assert.Equal(2, auction.Bids.Count);
            Assert.Equal(3, auction.Version);
        }

        [Theory]
        [InlineData(41000)]
        [InlineData(40500)]
        public void PlaceBid_NotAboveCurrentHighest_ThrowsAndRecordsNothing(decimal amount)
        {
            var auction = CreateOpenAuction(out _);
            auction.PlaceBid(CreateUser("Buyer A"), 41000, Now);

            Assert.Throws<InvalidOperationException>(() => auction.PlaceBid(CreateUser("Buyer B"), amount, Now.AddMinutes(1)));

            Assert.Equal(41000, auction.HighestBidAmount!.Amount);
            Assert.Single(auction.Bids);
            Assert.Equal(2, auction.Version);
        }

        [Fact]
        public void PlaceBid_FromSellersCompany_Throws()
        {
            var auction = CreateOpenAuction(out var seller);
            var colleague = User.Create(seller.Company, "Colleague");

            Assert.Throws<InvalidOperationException>(() => auction.PlaceBid(colleague, 40000, Now));
        }

        [Fact]
        public void PlaceBid_AfterEnd_Throws()
        {
            var auction = CreateOpenAuction(out _);

            Assert.Throws<InvalidOperationException>(() => auction.PlaceBid(CreateUser("Buyer Co"), 40000, EndsAt));
        }

        [Fact]
        public void Close_BeforeEnd_Throws()
        {
            var auction = CreateOpenAuction(out _);

            Assert.Throws<InvalidOperationException>(() => auction.Close(EndsAt.AddSeconds(-1)));
        }

        [Fact]
        public void Close_AfterEndWithBids_HasWinner()
        {
            var auction = CreateOpenAuction(out _);
            var bidder = CreateUser("Buyer Co");
            auction.PlaceBid(bidder, 40000, Now);

            auction.Close(EndsAt);

            Assert.Equal(AuctionStatus.Closed, auction.Status);
            Assert.True(auction.HasWinner);
            Assert.Equal(bidder.Id, auction.HighestBidderUserId);
        }

        [Fact]
        public void Close_AfterEndWithoutBids_HasNoWinner()
        {
            var auction = CreateOpenAuction(out _);

            auction.Close(EndsAt);

            Assert.Equal(AuctionStatus.Closed, auction.Status);
            Assert.False(auction.HasWinner);
        }

        [Fact]
        public void PlaceBid_OnClosedAuction_Throws()
        {
            var auction = CreateOpenAuction(out _);
            auction.Close(EndsAt);

            Assert.Throws<InvalidOperationException>(() => auction.PlaceBid(CreateUser("Buyer Co"), 40000, EndsAt));
        }

        [Fact]
        public void Cancel_WithoutBids_Cancels()
        {
            var auction = CreateOpenAuction(out _);

            auction.Cancel();

            Assert.Equal(AuctionStatus.Cancelled, auction.Status);
        }

        [Fact]
        public void Cancel_WithBids_Throws()
        {
            var auction = CreateOpenAuction(out _);
            auction.PlaceBid(CreateUser("Buyer Co"), 40000, Now);

            Assert.Throws<InvalidOperationException>(() => auction.Cancel());
        }
    }
}
