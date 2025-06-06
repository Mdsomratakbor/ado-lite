using Microsoft.Extensions.DependencyInjection;
using AdoLite.Core.Interfaces;
using AdoLite.Core.Enums;
using AdoLite.Postgres;
using AdoLite.Core.Services;
using AdoLite.SqlServer;
using AdoLite.MySql;
using Microsoft.Extensions.DependencyInjection.Extensions;


namespace AdoLite.Extension
{
    public static class DataContextServiceCollectionExtensions
    {
        public static IServiceCollection AddAdoLiteDataContext(
            this IServiceCollection services,
            DatabaseProvider providerType,
            string connectionString,
            ServiceLifetime lifetime = ServiceLifetime.Scoped)
        {

            // Register dependencies first
            //services.Add(new ServiceDescriptor(typeof(IDataJSONServices), typeof(DataJSONServices), lifetime));

            // Hack: Allow fallback registration of IDataJSONServices only if not already registered.
            // This enables unit tests or other consumers to inject mock implementations before calling this method.
            services.TryAdd(new ServiceDescriptor(typeof(IDataJSONServices), typeof(DataJSONServices), lifetime));

            // TODO: Register other dependencies like IDataQueryAsync, IDataTransaction, etc.

            Func<IServiceProvider, IDataContext> implementationFactory = providerType switch
            {
                DatabaseProvider.PostgreSQL => provider => new PostgresDataContext(
                    connectionString,
                    provider.GetRequiredService<IDataJSONServices>()
                ),
                DatabaseProvider.SqlServer => provider => new MsSqlDataContext(
                   connectionString,
                   provider.GetRequiredService<IDataJSONServices>()
               ),
                DatabaseProvider.MySQL => provider => new MySqlDataContext(
                 connectionString,
                 provider.GetRequiredService<IDataJSONServices>()
             ),
                _ => throw new NotSupportedException($"Database provider '{providerType}' is not supported.")
            };

            var descriptor = new ServiceDescriptor(typeof(IDataContext), implementationFactory, lifetime);
            services.Add(descriptor);

            return services;
        }

        public static IServiceCollection UsePostgreSQL(this IServiceCollection services, string connectionString, ServiceLifetime lifetime = ServiceLifetime.Scoped)
            => services.AddAdoLiteDataContext(DatabaseProvider.PostgreSQL, connectionString, lifetime);

        public static IServiceCollection UseSqlServer(this IServiceCollection services, string connectionString, ServiceLifetime lifetime = ServiceLifetime.Scoped)
            => services.AddAdoLiteDataContext(DatabaseProvider.SqlServer, connectionString, lifetime);

        public static IServiceCollection UseMySQL(this IServiceCollection services, string connectionString, ServiceLifetime lifetime = ServiceLifetime.Scoped)
            => services.AddAdoLiteDataContext(DatabaseProvider.MySQL, connectionString, lifetime);
    }
}
