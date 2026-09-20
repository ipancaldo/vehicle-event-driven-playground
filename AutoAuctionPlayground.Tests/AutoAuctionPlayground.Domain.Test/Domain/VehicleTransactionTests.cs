using AutoAuctionPlayground.Domain.Entities.Auctions;
using AutoAuctionPlayground.Domain.Entities.Companies;
using AutoAuctionPlayground.Domain.Entities.Transactions;
using AutoAuctionPlayground.Domain.Entities.Users;
using AutoAuctionPlayground.Domain.Entities.Vehicle;
using AutoAuctionPlayground.Domain.Enums;

namespace AutoAuctionPlayground.Tests.Domain
{
    public class VehicleTransactionTests
    {
        private static readonly DateTime Now = new(2026, 9, 20, 12, 0, 0, DateTimeKind.Utc);

        private static User CreateUser(string company) => User.Create(Company.Create(company), "Someone");

        private static VehicleListing CreateListing(User dealer, bool publish = true)
        {
            var model = VehicleMake.Create("Ford").AddModel("Focus", 1998);
            var listing = VehicleListing.Create(dealer, "ABC123", model, 2017, 127822, 42563);
            if (publish)
                listing.Publish();
            return listing;
        }

        [Fact]
        public void CreateDirectSale_FromPublishedListing_RecordsListingPrice()
        {
            var listing = CreateListing(CreateUser("Seller Co"));
            var buyer = CreateUser("Buyer Co");

            var transaction = VehicleTransaction.CreateDirectSale(listing, buyer, Now);

            Assert.Equal(listing.Id, transaction.VehicleListingId);
            Assert.Null(transaction.AuctionId);
            Assert.Equal(buyer.Id, transaction.BuyerUserId);
            Assert.Equal(42563, transaction.FinalPrice.Amount);
            Assert.Equal(VehicleTransactionKind.DirectSale, transaction.Kind);
            Assert.Equal(Now, transaction.FinalizedAt);
        }

        [Fact]
        public void CreateDirectSale_FromDraftListing_Throws()
        {
            var listing = CreateListing(CreateUser("Seller Co"), publish: false);

            Assert.Throws<InvalidOperationException>(() =>
                VehicleTransaction.CreateDirectSale(listing, CreateUser("Buyer Co"), Now));
        }

        [Fact]
        public void CreateDirectSale_BySellersOwnCompany_Throws()
        {
            var seller = CreateUser("Seller Co");
            var listing = CreateListing(seller);
            var colleague = User.Create(seller.Company, "Colleague");

            Assert.Throws<InvalidOperationException>(() =>
                VehicleTransaction.CreateDirectSale(listing, colleague, Now));
        }

        [Fact]
        public void CreateFromAuction_WithWinner_RecordsWinningBid()
        {
            var listing = CreateListing(CreateUser("Seller Co"));
            var auction = Auction.Create(listing, 40000, Now, Now.AddDays(1));
            var winner = CreateUser("Buyer Co");
            auction.PlaceBid(winner, 45000, Now);
            auction.Close(Now.AddDays(1));

            var transaction = VehicleTransaction.CreateFromAuction(auction, Now.AddDays(1));

            Assert.Equal(listing.Id, transaction.VehicleListingId);
            Assert.Equal(auction.Id, transaction.AuctionId);
            Assert.Equal(winner.Id, transaction.BuyerUserId);
            Assert.Equal(45000, transaction.FinalPrice.Amount);
            Assert.Equal(VehicleTransactionKind.AuctionWin, transaction.Kind);
        }

        [Fact]
        public void CreateFromAuction_StillOpen_Throws()
        {
            var auction = Auction.Create(CreateListing(CreateUser("Seller Co")), 40000, Now, Now.AddDays(1));
            auction.PlaceBid(CreateUser("Buyer Co"), 45000, Now);

            Assert.Throws<InvalidOperationException>(() => VehicleTransaction.CreateFromAuction(auction, Now));
        }

        [Fact]
        public void CreateFromAuction_ClosedWithoutBids_Throws()
        {
            var auction = Auction.Create(CreateListing(CreateUser("Seller Co")), 40000, Now, Now.AddDays(1));
            auction.Close(Now.AddDays(1));

            Assert.Throws<InvalidOperationException>(() => VehicleTransaction.CreateFromAuction(auction, Now.AddDays(1)));
        }
    }
}
