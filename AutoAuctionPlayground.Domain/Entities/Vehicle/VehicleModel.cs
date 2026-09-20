namespace AutoAuctionPlayground.Domain.Entities.Vehicle
{
    public class VehicleModel
    {
        private const int EarliestProductionYear = 1886;

        public Guid Id { get; private set; }
        public Guid VehicleMakeId { get; private set; }
        public VehicleMake Make { get; private set; } = default!;
        public string Name { get; private set; } = default!;
        public int ProductionStartYear { get; private set; }
        public int? ProductionEndYear { get; private set; }
        public bool IsActive { get; private set; }

        private VehicleModel() { }
        private VehicleModel(
            VehicleMake make,
            string name,
            int productionStartYear,
            int? productionEndYear)
        {
            ValidateProductionYears(productionStartYear, productionEndYear);

            Id = Guid.NewGuid();
            VehicleMakeId = make.Id;
            Make = make;
            Name = name;
            ProductionStartYear = productionStartYear;
            ProductionEndYear = productionEndYear;
            IsActive = true;
        }

        internal static VehicleModel Create(
            VehicleMake make,
            string name,
            int productionStartYear,
            int? productionEndYear)
        {
            ArgumentNullException.ThrowIfNull(make);

            return new VehicleModel(
                make,
                name,
                productionStartYear,
                productionEndYear);
        }

        public bool WasProducedIn(int year) =>
            year >= ProductionStartYear &&
            (ProductionEndYear is null || year <= ProductionEndYear);

        internal void EnsureSelectableFor(int year)
        {
            Make.EnsureActive();
            EnsureActive();

            if (!WasProducedIn(year))
            {
                throw new ArgumentOutOfRangeException(
                    nameof(year),
                    "Year must fall within the selected vehicle model's production range.");
            }
        }

        internal void EnsureActive(string? message = null)
        {
            if (!IsActive)
                throw new InvalidOperationException(
                    message ?? "The selected vehicle model is inactive.");
        }

        internal void Deactivate() => IsActive = false;

        private static void ValidateProductionYears(int startYear, int? endYear)
        {
            if (startYear < EarliestProductionYear)
                throw new ArgumentOutOfRangeException(
                    nameof(startYear),
                    $"Production cannot start before {EarliestProductionYear}.");

            if (endYear < startYear)
                throw new ArgumentOutOfRangeException(
                    nameof(endYear),
                    "Production end year cannot be earlier than the start year.");
        }
    }
}
