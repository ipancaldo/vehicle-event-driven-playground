using AutoAuctionPlayground.Domain.Entities.Vehicle;

namespace AutoAuctionPlayground.Tests.Domain
{
    public class VehicleMakeTests
    {
        [Fact]
        public void Create_WithValidName_CreatesActiveMake()
        {
            var make = VehicleMake.Create("  Ford  ");

            Assert.NotEqual(Guid.Empty, make.Id);
            Assert.Equal("Ford", make.Name);
            Assert.True(make.IsActive);
            Assert.Empty(make.Models);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Create_WithInvalidName_Throws(string? name)
        {
            Assert.Throws<ArgumentException>(() => VehicleMake.Create(name!));
        }

        [Fact]
        public void AddModel_AssignsModelToMakeAndNormalizesName()
        {
            var make = VehicleMake.Create("Ford");

            var model = make.AddModel("  Focus  ", 1998);

            Assert.NotEqual(Guid.Empty, model.Id);
            Assert.Equal(make.Id, model.VehicleMakeId);
            Assert.Same(make, model.Make);
            Assert.Equal("Focus", model.Name);
            Assert.Equal(1998, model.ProductionStartYear);
            Assert.Null(model.ProductionEndYear);
            Assert.True(model.IsActive);
            Assert.Contains(model, make.Models);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void AddModel_WithInvalidName_Throws(string? name)
        {
            var make = VehicleMake.Create("Ford");

            Assert.Throws<ArgumentException>(() =>
                make.AddModel(name!, 1998));
        }

        [Fact]
        public void AddModel_WithDuplicateNameForSameMake_ThrowsIgnoringCase()
        {
            var make = VehicleMake.Create("Ford");
            make.AddModel("Focus", 1998);

            Assert.Throws<InvalidOperationException>(() =>
                make.AddModel("focus", 1998));
        }

        [Fact]
        public void AddModel_WithProductionEndBeforeStart_Throws()
        {
            var make = VehicleMake.Create("Ford");

            Assert.Throws<ArgumentOutOfRangeException>(() =>
                make.AddModel("Focus", 2000, 1999));
        }

        [Fact]
        public void AddModel_WithProductionStartBeforeFirstAutomobile_Throws()
        {
            var make = VehicleMake.Create("Ford");

            Assert.Throws<ArgumentOutOfRangeException>(() =>
                make.AddModel("Focus", 1885));
        }

        [Fact]
        public void AddModel_ToInactiveMake_Throws()
        {
            var make = VehicleMake.Create("Ford");
            make.Deactivate();

            Assert.Throws<InvalidOperationException>(() =>
                make.AddModel("Focus", 1998));
        }

        [Theory]
        [InlineData(1997, false)]
        [InlineData(1998, true)]
        [InlineData(2018, true)]
        [InlineData(2019, false)]
        public void WasProducedIn_ForClosedProductionRange_ReturnsExpected(
            int year,
            bool expected)
        {
            var make = VehicleMake.Create("Ford");
            var model = make.AddModel("Focus", 1998, 2018);

            Assert.Equal(expected, model.WasProducedIn(year));
        }

        [Fact]
        public void WasProducedIn_WithOpenProductionRange_AcceptsLaterYear()
        {
            var make = VehicleMake.Create("Honda");
            var model = make.AddModel("Accord", 1976);

            Assert.True(model.WasProducedIn(2030));
        }

        [Fact]
        public void DeactivateModel_DeactivatesOwnedModel()
        {
            var make = VehicleMake.Create("Ford");
            var model = make.AddModel("Focus", 1998);

            make.DeactivateModel(model.Id);

            Assert.False(model.IsActive);
        }

        [Fact]
        public void DeactivateModel_ForDifferentMake_Throws()
        {
            var ford = VehicleMake.Create("Ford");
            var honda = VehicleMake.Create("Honda");
            var accord = honda.AddModel("Accord", 1976);

            Assert.Throws<InvalidOperationException>(() =>
                ford.DeactivateModel(accord.Id));
        }
    }
}
