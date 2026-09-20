using AutoAuctionPlayground.Domain.Entities.Companies;
using AutoAuctionPlayground.Domain.Entities.Users;
using AutoAuctionPlayground.Domain.Entities.Vehicle;
using AutoAuctionPlayground.Domain.Enums;

namespace AutoAuctionPlayground.Tests.Domain
{
    public class VehicleListingTests
    {
        private const string ValidVin = "ABC123";

        private static User CreateDealer() => User.Create(Company.Create("Ford Dealer"), "Alice");

        private static VehicleListing CreateValidListing(VehicleListingStatus status = VehicleListingStatus.Draft)
        {
            var make = VehicleMake.Create("Ford");
            var model = make.AddModel("Focus", 1998);
            var listing = VehicleListing.Create(
                dealer: CreateDealer(),
                vin: ValidVin,
                vehicleModel: model,
                year: 2017,
                mileage: 127822,
                price: 42563);

            switch (status)
            {
                case VehicleListingStatus.Published:
                    listing.Publish();
                    break;
                case VehicleListingStatus.Sold:
                    listing.Publish();
                    listing.MarkAsSold();
                    break;
                case VehicleListingStatus.Cancelled:
                    listing.Cancel();
                    break;
            }

            return listing;
        }

        [Fact]
        public void Handle_CreateVehicleListing_Succeeds()
        {
            var dealer = CreateDealer();
            var vin = "ABC123";
            var make = VehicleMake.Create("Ford");
            var model = make.AddModel("Focus", 1998);
            var year = 2017;
            var mileageKm = 127822;
            var price = 42563;

            var vehicleListing = VehicleListing.Create(
                dealer,
                vin,
                model,
                year,
                mileageKm,
                price
            );

            Assert.NotEqual(Guid.Empty, vehicleListing.Id);
            Assert.Equal(dealer.Id, vehicleListing.DealerId);
            Assert.Equal(dealer.CompanyId, vehicleListing.DealerCompanyId);
            Assert.Empty(vehicleListing.PriceHistory);
            Assert.Equal(model.Id, vehicleListing.VehicleModelId);
            Assert.Same(model, vehicleListing.Model);
            Assert.Equal(vin, vehicleListing.Details.Vin);
            Assert.Equal("Focus", vehicleListing.Model.Name);
            Assert.Equal("Ford", vehicleListing.Model.Make.Name);
            Assert.Equal(year, vehicleListing.Details.Year);
            Assert.Equal(mileageKm, vehicleListing.Details.MileageKm);
            Assert.Equal(price, vehicleListing.Details.Price);
            Assert.Equal(VehicleListingStatus.Draft, vehicleListing.Status);
            Assert.Null(vehicleListing.UpdatedAt);
            Assert.InRange(vehicleListing.CreatedAt, DateTime.UtcNow.AddMinutes(-1), DateTime.UtcNow);
        }

        [Fact]
        public void Create_WithNullDealer_Throws()
        {
            var make = VehicleMake.Create("Ford");
            var model = make.AddModel("Focus", 1998);

            Assert.Throws<ArgumentNullException>(() => VehicleListing.Create(
                null!, ValidVin, model, 2017, 127822, 42563));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Create_WithInvalidVin_Throws(string? vin)
        {
            var make = VehicleMake.Create("Ford");
            var model = make.AddModel("Focus", 1998);

            Assert.Throws<ArgumentException>(() => VehicleListing.Create(
                CreateDealer(), vin!, model, 2017, 127822, 42563));
        }

        [Fact]
        public void Create_WithNullModel_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => VehicleListing.Create(
                CreateDealer(), ValidVin, null!, 2017, 127822, 42563));
        }

        [Fact]
        public void Create_WithInactiveMake_Throws()
        {
            var make = VehicleMake.Create("Ford");
            var model = make.AddModel("Focus", 1998);
            make.Deactivate();

            Assert.Throws<InvalidOperationException>(() => VehicleListing.Create(
                CreateDealer(), ValidVin, model, 2017, 127822, 42563));
        }

        [Fact]
        public void Create_WithInactiveModel_Throws()
        {
            var make = VehicleMake.Create("Ford");
            var model = make.AddModel("Focus", 1998);
            make.DeactivateModel(model.Id);

            Assert.Throws<InvalidOperationException>(() => VehicleListing.Create(
                CreateDealer(), ValidVin, model, 2017, 127822, 42563));
        }

        [Fact]
        public void Create_WithYearOutsideModelProductionRange_Throws()
        {
            var make = VehicleMake.Create("Ford");
            var model = make.AddModel("Focus", 1998, 2018);

            Assert.Throws<ArgumentOutOfRangeException>(() => VehicleListing.Create(
                CreateDealer(), ValidVin, model, 2019, 127822, 42563));
        }

        [Fact]
        public void Create_WithYearTooOld_Throws()
        {
            var make = VehicleMake.Create("Ford");
            var model = make.AddModel("Focus", 1998);

            Assert.Throws<ArgumentOutOfRangeException>(() => VehicleListing.Create(
                CreateDealer(), ValidVin, model, 1899, 127822, 42563));
        }

        [Fact]
        public void Create_WithYearTooFarInFuture_Throws()
        {
            var invalidYear = DateTime.UtcNow.Year + 2;
            var make = VehicleMake.Create("Ford");
            var model = make.AddModel("Focus", 1998);

            Assert.Throws<ArgumentOutOfRangeException>(() => VehicleListing.Create(
                CreateDealer(), ValidVin, model, invalidYear, 127822, 42563));
        }

        [Fact]
        public void Create_WithNegativeMileage_Throws()
        {
            var make = VehicleMake.Create("Ford");
            var model = make.AddModel("Focus", 1998);

            Assert.Throws<ArgumentOutOfRangeException>(() => VehicleListing.Create(
                CreateDealer(), ValidVin, model, 2017, -1, 42563));
        }

        [Fact]
        public void Create_WithNegativePrice_Throws()
        {
            var make = VehicleMake.Create("Ford");
            var model = make.AddModel("Focus", 1998);

            Assert.Throws<ArgumentOutOfRangeException>(() => VehicleListing.Create(
                CreateDealer(), ValidVin, model, 2017, 127822, -1));
        }

        [Fact]
        public void UpdatePrice_WithValidPrice_UpdatesPriceAndTouches()
        {
            var listing = CreateValidListing();
            var changedBy = Guid.NewGuid();

            listing.UpdatePrice(50000, changedBy);

            Assert.Equal(50000, listing.Details.Price);
            Assert.NotNull(listing.UpdatedAt);

            var change = Assert.Single(listing.PriceHistory);
            Assert.Equal(42563, change.OldPrice);
            Assert.Equal(50000, change.NewPrice);
            Assert.Equal(changedBy, change.ChangedByUserId);
            Assert.Equal(listing.Id, change.VehicleListingId);
        }

        [Fact]
        public void UpdatePrice_WithNegativePrice_Throws()
        {
            var listing = CreateValidListing();

            Assert.Throws<ArgumentOutOfRangeException>(() => listing.UpdatePrice(-1, Guid.NewGuid()));
        }

        [Fact]
        public void UpdatePrice_OnCancelledListing_Throws()
        {
            var listing = CreateValidListing(VehicleListingStatus.Cancelled);

            Assert.Throws<InvalidOperationException>(() => listing.UpdatePrice(50000, Guid.NewGuid()));
        }

        [Fact]
        public void UpdateMileage_WithHigherMileage_UpdatesMileageAndTouches()
        {
            var listing = CreateValidListing();

            listing.UpdateMileage(130000);

            Assert.Equal(130000, listing.Details.MileageKm);
            Assert.NotNull(listing.UpdatedAt);
        }

        [Fact]
        public void UpdateMileage_WithLowerMileage_Throws()
        {
            var listing = CreateValidListing();

            Assert.Throws<ArgumentOutOfRangeException>(() => listing.UpdateMileage(100000));
        }

        [Fact]
        public void UpdateMileage_OnCancelledListing_Throws()
        {
            var listing = CreateValidListing(VehicleListingStatus.Cancelled);

            Assert.Throws<InvalidOperationException>(() => listing.UpdateMileage(130000));
        }

        [Fact]
        public void Publish_FromDraft_TransitionsToPublished()
        {
            var listing = CreateValidListing();

            listing.Publish();

            Assert.Equal(VehicleListingStatus.Published, listing.Status);
            Assert.NotNull(listing.UpdatedAt);
        }

        [Fact]
        public void Publish_AlreadyPublished_Throws()
        {
            var listing = CreateValidListing(VehicleListingStatus.Published);

            Assert.Throws<InvalidOperationException>(() => listing.Publish());
        }

        [Fact]
        public void Publish_CancelledListing_Throws()
        {
            var listing = CreateValidListing(VehicleListingStatus.Cancelled);

            Assert.Throws<InvalidOperationException>(() => listing.Publish());
        }

        [Fact]
        public void MarkAsSold_FromPublished_TransitionsToSold()
        {
            var listing = CreateValidListing(VehicleListingStatus.Published);

            listing.MarkAsSold();

            Assert.Equal(VehicleListingStatus.Sold, listing.Status);
            Assert.NotNull(listing.UpdatedAt);
        }

        [Fact]
        public void MarkAsSold_FromDraft_Throws()
        {
            var listing = CreateValidListing();

            Assert.Throws<InvalidOperationException>(() => listing.MarkAsSold());
        }

        [Fact]
        public void Cancel_FromDraft_TransitionsToCancelled()
        {
            var listing = CreateValidListing();

            listing.Cancel();

            Assert.Equal(VehicleListingStatus.Cancelled, listing.Status);
        }

        [Fact]
        public void Cancel_FromSold_Throws()
        {
            // Sold has no outgoing transitions in the status table, so it's terminal:
            // even Cancel() must be rejected once a listing has sold.
            var listing = CreateValidListing(VehicleListingStatus.Sold);

            Assert.Throws<InvalidOperationException>(() => listing.Cancel());
        }

        [Fact]
        public void Cancel_AlreadyCancelled_Throws()
        {
            var listing = CreateValidListing(VehicleListingStatus.Cancelled);

            Assert.Throws<InvalidOperationException>(() => listing.Cancel());
        }

        [Fact]
        public void Pause_FromPublished_TransitionsToPaused()
        {
            var listing = CreateValidListing(VehicleListingStatus.Published);

            listing.Pause();

            Assert.Equal(VehicleListingStatus.Paused, listing.Status);
        }

        [Fact]
        public void Pause_FromDraft_Throws()
        {
            var listing = CreateValidListing();

            Assert.Throws<InvalidOperationException>(() => listing.Pause());
        }

        [Fact]
        public void Resume_FromPaused_TransitionsToPublished()
        {
            var listing = CreateValidListing(VehicleListingStatus.Published);
            listing.Pause();

            listing.Resume();

            Assert.Equal(VehicleListingStatus.Published, listing.Status);
        }

        [Fact]
        public void Resume_FromDraft_Throws()
        {
            var listing = CreateValidListing();

            Assert.Throws<InvalidOperationException>(() => listing.Resume());
        }

        [Fact]
        public void EnsurePublished_WhenPublished_DoesNotThrow()
        {
            var listing = CreateValidListing(VehicleListingStatus.Published);

            listing.EnsurePublished();
        }

        [Theory]
        [InlineData(VehicleListingStatus.Draft)]
        [InlineData(VehicleListingStatus.Sold)]
        [InlineData(VehicleListingStatus.Cancelled)]
        public void EnsurePublished_WhenNotPublished_Throws(VehicleListingStatus status)
        {
            var listing = CreateValidListing(status);

            Assert.Throws<InvalidOperationException>(() => listing.EnsurePublished());
        }
    }
}
