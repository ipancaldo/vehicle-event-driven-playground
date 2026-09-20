using AutoAuctionPlayground.Application.Interfaces.Queries;
using AutoAuctionPlayground.Application.Interfaces.Repositories;
using AutoAuctionPlayground.Infrastructure.Persistence;
using AutoAuctionPlayground.Infrastructure.Persistence.Factories;
using AutoAuctionPlayground.Infrastructure.Persistence.Queries;
using AutoAuctionPlayground.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AutoAuctionPlayground.Infrastructure.Extensions
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException(
                    "ConnectionStrings:DefaultConnection is not configured. For local development set it with " +
                    "'dotnet user-secrets set \"ConnectionStrings:DefaultConnection\" \"...\"' in the API project.");

            services.AddDbContext<AutoAuctionDbContext>(options => options.UseNpgsql(connectionString));

            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<IRepositoryFactory, RepositoryFactory>();
            services.AddScoped<IVehicleListingQueries, VehicleListingQueries>();

            // Every concrete repository is registered by its interfaces (ICompanyRepository, ...),
            // so the RepositoryFactory can resolve them by interface from the current scope.
            services.Scan(scan => scan
                .FromAssemblies(typeof(BaseRepository<>).Assembly)
                .AddClasses(classes => classes.AssignableTo(typeof(IBaseRepository<>)))
                    .AsImplementedInterfaces()
                    .WithScopedLifetime());

            return services;
        }
    }
}
