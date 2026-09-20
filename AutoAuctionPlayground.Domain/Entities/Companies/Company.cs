using AutoAuctionPlayground.Domain.Entities.Users;

namespace AutoAuctionPlayground.Domain.Entities.Companies
{
    public class Company
    {
        private readonly List<User> _users = [];

        public Guid Id { get; private set; }
        public string Name { get; private set; } = default!;
        public DateTime CreatedAt { get; private set; }

        // Read-only navigation for queries. User is its own aggregate: users are created through
        // User.Create, never through Company, so nothing here ever mutates this list.
        public IReadOnlyCollection<User> Users => _users.AsReadOnly();

        private Company() { }
        private Company(string name)
        {
            Id = Guid.NewGuid();
            Name = name;
            CreatedAt = DateTime.UtcNow;
        }

        public static Company Create(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Company name is required.", nameof(name));

            return new Company(name.Trim());
        }
    }
}
