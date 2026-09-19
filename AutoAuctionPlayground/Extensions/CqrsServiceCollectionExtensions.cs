using AutoAuctionPlayground.Application.CQRS.Abstractions;
using AutoAuctionPlayground.Application.CQRS.Dispatchers;

namespace AutoAuctionPlayground.Web.Extensions
{
    public static class CqrsServiceCollectionExtensions
    {
        /// <summary>
        /// Registers CQRS command/query handlers and dispatchers.
        /// </summary>
        /// <param name="services">DI container</param>
        public static IServiceCollection AddCQRS(this IServiceCollection services)
        {
            var appAssembly = typeof(ICommandHandler<>).Assembly;

            // Scan & register every ICommandHandler<,>, ICommandHandler<> and IQueryHandler<,>
            // found in the Application assembly, so handlers never need to be registered by hand.
            services.Scan(scan => scan
                .FromAssemblies(appAssembly)

                .AddClasses(c => c.AssignableTo(typeof(ICommandHandler<,>)))
                    .AsImplementedInterfaces()
                    .WithScopedLifetime()

                .AddClasses(c => c.AssignableTo(typeof(ICommandHandler<>)))
                    .AsImplementedInterfaces()
                    .WithScopedLifetime()

                .AddClasses(c => c.AssignableTo(typeof(IQueryHandler<,>)))
                    .AsImplementedInterfaces()
                    .WithScopedLifetime()
            );

            services.AddScoped<ICommandDispatcher, CommandDispatcher>();
            services.AddScoped<IQueryDispatcher, QueryDispatcher>();

            return services;
        }
    }
}
