using AutoAuctionPlayground.Domain.Entities;
using AutoAuctionPlayground.Domain.Enums;

namespace AutoAuctionPlayground.Tests.Domain
{
    public class VehicleListingTests
    {
        private const string ValidVin = "ABC123";

        private static VehicleListing CreateValidListing(VehicleListingStatus status = VehicleListingStatus.Draft)
        {
            var listing = VehicleListing.Create(
                dealerId: Guid.NewGuid(),
                vin: ValidVin,
                make: "Ford",
                model: "Focus",
                year: 2017,
                mileage: 127822,
                price: 42563);

            switch (status)
            {
                case VehicleListingStatus.Active:
                    listing.Publish();
                    break;
                case VehicleListingStatus.Sold:
                    listing.Publish();
                    listing.MarkAsSold();
                    break;
                case VehicleListingStatus.Removed:
                    listing.Remove();
                    break;
            }

            return listing;
        }

        [Fact]
        public void Handle_CreateVehicleListing_Succeeds()
        {
            var dealerId = Guid.NewGuid();
            var vin = "ABC123";
            var make = "Ford";
            var model = "Focus";
            var year = 2017;
            var mileageKm = 127822;
            var price = 42563;

            var vehicleListing = VehicleListing.Create(
                dealerId,
                vin,
                make,
                model,
                year,
                mileageKm,
                price
            );

            Assert.NotEqual(Guid.Empty, vehicleListing.Id);
            Assert.Equal(dealerId, vehicleListing.DealerId);
            Assert.Equal(vin, vehicleListing.VIN);
            Assert.Equal(make, vehicleListing.Make);
            Assert.Equal(model, vehicleListing.Model);
            Assert.Equal(year, vehicleListing.Year);
            Assert.Equal(mileageKm, vehicleListing.MileageKm);
            Assert.Equal(price, vehicleListing.Price);
            Assert.Equal(VehicleListingStatus.Draft, vehicleListing.Status);
            Assert.Null(vehicleListing.UpdatedAt);
            Assert.InRange(vehicleListing.CreatedAt, DateTime.UtcNow.AddMinutes(-1), DateTime.UtcNow);
        }

        [Fact]
        public void Create_WithEmptyDealerId_Throws()
        {
            Assert.Throws<ArgumentException>(() => VehicleListing.Create(
                Guid.Empty, ValidVin, "Ford", "Focus", 2017, 127822, 42563));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Create_WithInvalidVin_Throws(string? vin)
        {
            Assert.Throws<ArgumentException>(() => VehicleListing.Create(
                Guid.NewGuid(), vin!, "Ford", "Focus", 2017, 127822, 42563));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Create_WithInvalidMake_Throws(string? make)
        {
            Assert.Throws<ArgumentException>(() => VehicleListing.Create(
                Guid.NewGuid(), ValidVin, make!, "Focus", 2017, 127822, 42563));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Create_WithInvalidModel_Throws(string? model)
        {
            Assert.Throws<ArgumentException>(() => VehicleListing.Create(
                Guid.NewGuid(), ValidVin, "Ford", model!, 2017, 127822, 42563));
        }

        [Fact]
        public void Create_WithYearTooOld_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => VehicleListing.Create(
                Guid.NewGuid(), ValidVin, "Ford", "Focus", 1899, 127822, 42563));
        }

        [Fact]
        public void Create_WithYearTooFarInFuture_Throws()
        {
            var invalidYear = DateTime.UtcNow.Year + 2;

            Assert.Throws<ArgumentOutOfRangeException>(() => VehicleListing.Create(
                Guid.NewGuid(), ValidVin, "Ford", "Focus", invalidYear, 127822, 42563));
        }

        [Fact]
        public void Create_WithNegativeMileage_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => VehicleListing.Create(
                Guid.NewGuid(), ValidVin, "Ford", "Focus", 2017, -1, 42563));
        }

        [Fact]
        public void Create_WithNegativePrice_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => VehicleListing.Create(
                Guid.NewGuid(), ValidVin, "Ford", "Focus", 2017, 127822, -1));
        }

        [Fact]
        public void UpdatePrice_WithValidPrice_UpdatesPriceAndTouches()
        {
            var listing = CreateValidListing();

            listing.UpdatePrice(50000);

            Assert.Equal(50000, listing.Price);
            Assert.NotNull(listing.UpdatedAt);
        }

        [Fact]
        public void UpdatePrice_WithNegativePrice_Throws()
        {
            var listing = CreateValidListing();

            Assert.Throws<ArgumentOutOfRangeException>(() => listing.UpdatePrice(-1));
        }

        [Fact]
        public void UpdatePrice_OnRemovedListing_Throws()
        {
            var listing = CreateValidListing(VehicleListingStatus.Removed);

            Assert.Throws<InvalidOperationException>(() => listing.UpdatePrice(50000));
        }

        [Fact]
        public void UpdateMileage_WithHigherMileage_UpdatesMileageAndTouches()
        {
            var listing = CreateValidListing();

            listing.UpdateMileage(130000);

            Assert.Equal(130000, listing.MileageKm);
            Assert.NotNull(listing.UpdatedAt);
        }

        [Fact]
        public void UpdateMileage_WithLowerMileage_Throws()
        {
            var listing = CreateValidListing();

            Assert.Throws<ArgumentOutOfRangeException>(() => listing.UpdateMileage(100000));
        }

        [Fact]
        public void UpdateMileage_OnRemovedListing_Throws()
        {
            var listing = CreateValidListing(VehicleListingStatus.Removed);

            Assert.Throws<InvalidOperationException>(() => listing.UpdateMileage(130000));
        }

        [Fact]
        public void Publish_FromDraft_TransitionsToActive()
        {
            var listing = CreateValidListing();

            listing.Publish();

            Assert.Equal(VehicleListingStatus.Active, listing.Status);
            Assert.NotNull(listing.UpdatedAt);
        }

        [Fact]
        public void Publish_AlreadyActive_Throws()
        {
            var listing = CreateValidListing(VehicleListingStatus.Active);

            Assert.Throws<InvalidOperationException>(() => listing.Publish());
        }

        [Fact]
        public void Publish_RemovedListing_Throws()
        {
            var listing = CreateValidListing(VehicleListingStatus.Removed);

            Assert.Throws<InvalidOperationException>(() => listing.Publish());
        }

        [Fact]
        public void MarkAsSold_FromActive_TransitionsToSold()
        {
            var listing = CreateValidListing(VehicleListingStatus.Active);

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
        public void Remove_FromDraft_TransitionsToRemoved()
        {
            var listing = CreateValidListing();

            listing.Remove();

            Assert.Equal(VehicleListingStatus.Removed, listing.Status);
        }

        [Fact]
        public void Remove_FromSold_Throws()
        {
            // Sold has no outgoing transitions in the status table, so it's terminal:
            // even Remove() must be rejected once a listing has sold.
            var listing = CreateValidListing(VehicleListingStatus.Sold);

            Assert.Throws<InvalidOperationException>(() => listing.Remove());
        }

        [Fact]
        public void Remove_AlreadyRemoved_Throws()
        {
            var listing = CreateValidListing(VehicleListingStatus.Removed);

            Assert.Throws<InvalidOperationException>(() => listing.Remove());
        }
    }
}
