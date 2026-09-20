using AutoAuctionPlayground.Domain.Entities.Companies;

namespace AutoAuctionPlayground.Domain.Entities.Users
{
    public class User
    {
        public Guid Id { get; private set; }
        public Guid CompanyId { get; private set; }
        public Company Company { get; private set; } = default!;
        public string Name { get; private set; } = default!;
        public DateTime CreatedAt { get; private set; }

        private User() { }
        private User(Company company, string name)
        {
            Id = Guid.NewGuid();
            CompanyId = company.Id;
            Company = company;
            Name = name;
            CreatedAt = DateTime.UtcNow;
        }

        public static User Create(Company company, string name)
        {
            ArgumentNullException.ThrowIfNull(company);
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("User name is required.", nameof(name));

            return new User(company, name.Trim());
        }
    }
}
