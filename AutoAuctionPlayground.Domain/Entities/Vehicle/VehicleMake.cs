namespace AutoAuctionPlayground.Domain.Entities.Vehicle
{
    public class VehicleMake
    {
        private readonly List<VehicleModel> _models = [];

        public Guid Id { get; private set; }
        public string Name { get; private set; } = default!;
        public bool IsActive { get; private set; }
        public IReadOnlyCollection<VehicleModel> Models => _models.AsReadOnly();

        private VehicleMake() { }
        private VehicleMake(string name)
        {
            Id = Guid.NewGuid();
            Name = NormalizeName(name, nameof(name));
            IsActive = true;
        }

        public static VehicleMake Create(string name) => new(name);

        public VehicleModel AddModel(
            string name,
            int productionStartYear,
            int? productionEndYear = null)
        {
            EnsureActive("Cannot add a model to an inactive vehicle make.");

            var normalizedName = NormalizeName(name, nameof(name));

            if (_models.Any(model =>
                    string.Equals(model.Name, normalizedName, StringComparison.OrdinalIgnoreCase)))
            {
                throw new InvalidOperationException(
                    $"Vehicle model '{normalizedName}' already exists for make '{Name}'.");
            }

            var model = VehicleModel.Create(
                this,
                normalizedName,
                productionStartYear,
                productionEndYear);

            _models.Add(model);
            return model;
        }

        public void Deactivate() => IsActive = false;

        public void DeactivateModel(Guid modelId)
        {
            var model = _models.SingleOrDefault(candidate => candidate.Id == modelId)
                ?? throw new InvalidOperationException(
                    $"Vehicle model '{modelId}' does not belong to make '{Name}'.");

            model.Deactivate();
        }

        internal void EnsureActive(string? message = null)
        {
            if (!IsActive)
                throw new InvalidOperationException(
                    message ?? "The selected vehicle make is inactive.");
        }

        private static string NormalizeName(string name, string parameterName)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("A vehicle make or model name is required.", parameterName);

            return name.Trim();
        }
    }
}
