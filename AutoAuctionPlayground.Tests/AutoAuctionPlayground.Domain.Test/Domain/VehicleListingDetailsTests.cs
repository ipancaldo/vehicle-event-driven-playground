using AutoAuctionPlayground.Domain.ValueObjects;

namespace AutoAuctionPlayground.Tests.Domain
{
    public class VehicleListingDetailsTests
    {
        [Fact]
        public void Create_WithSameValues_ProducesEqualDetails()
        {
            var first = VehicleListingDetails.Create("ABC123", 2017, 127822, 42563);
            var second = VehicleListingDetails.Create("ABC123", 2017, 127822, 42563);

            Assert.Equal(first, second);
            Assert.NotSame(first, second);
        }

        [Fact]
        public void Create_WithDifferentValues_ProducesDifferentDetails()
        {
            var first = VehicleListingDetails.Create("ABC123", 2017, 127822, 42563);
            var second = VehicleListingDetails.Create("ABC123", 2017, 127823, 42563);

            Assert.NotEqual(first, second);
        }

        [Fact]
        public void Create_NormalizesVin()
        {
            var details = VehicleListingDetails.Create("  abc123 ", 2017, 127822, 42563);

            Assert.Equal("ABC123", details.Vin);
            Assert.Equal(details, VehicleListingDetails.Create("ABC123", 2017, 127822, 42563));
        }

        [Theory]
        [InlineData(1899)]
        [InlineData(3000)]
        public void Create_WithInvalidYear_Throws(int year)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                VehicleListingDetails.Create("ABC123", year, 127822, 42563));
        }

        [Fact]
        public void UpdateMileage_ReturnsUpdatedCopyAndPreservesOriginal()
        {
            var original = VehicleListingDetails.Create("ABC123", 2017, 127822, 42563);

            var updated = original.UpdateMileage(130000);

            Assert.Equal(127822, original.MileageKm);
            Assert.Equal(130000, updated.MileageKm);
            Assert.Equal(original.Vin, updated.Vin);
            Assert.Equal(original.Year, updated.Year);
            Assert.Equal(original.Price, updated.Price);
        }

        [Fact]
        public void UpdatePrice_ReturnsUpdatedCopyAndPreservesOriginal()
        {
            var original = VehicleListingDetails.Create("ABC123", 2017, 127822, 42563);

            var updated = original.UpdatePrice(50000);

            Assert.Equal(42563, original.Price.Amount);
            Assert.Equal(50000, updated.Price.Amount);
            Assert.Equal(original.Vin, updated.Vin);
            Assert.Equal(original.Year, updated.Year);
            Assert.Equal(original.MileageKm, updated.MileageKm);
        }

        [Fact]
        public void Update_WithValidValues_ReturnsComparableUpdatedDetails()
        {
            var original = VehicleListingDetails.Create("ABC123", 2017, 127822, 42563);

            var updated = original.Update("XYZ789", 2018, 130000, 50000);
            var expected = VehicleListingDetails.Create("XYZ789", 2018, 130000, 50000);

            Assert.Equal(expected, updated);
            Assert.NotEqual(original, updated);
        }

        [Fact]
        public void UpdateMileage_WithLowerMileage_Throws()
        {
            var details = VehicleListingDetails.Create("ABC123", 2017, 127822, 42563);

            Assert.Throws<ArgumentOutOfRangeException>(() =>
                details.UpdateMileage(100000));
        }

        [Fact]
        public void UpdatePrice_WithNegativePrice_Throws()
        {
            var details = VehicleListingDetails.Create("ABC123", 2017, 127822, 42563);

            Assert.Throws<ArgumentOutOfRangeException>(() =>
                details.UpdatePrice(-1));
        }
    }
}
