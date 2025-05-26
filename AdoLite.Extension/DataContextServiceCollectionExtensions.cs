using Microsoft.Extensions.DependencyInjection;
using AdoLite.Core.Interfaces;
using AdoLite.Common.Enums;
using AdoLite.Postgres;
using AdoLite.Core.Services;


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
            services.Add(new ServiceDescriptor(typeof(IDataJSONServices), typeof(DataJSONServices), lifetime));
            // TODO: Register other dependencies like IDataQueryAsync, IDataTransaction, etc.

            Func<IServiceProvider, IDataContext> implementationFactory = providerType switch
            {
                DatabaseProvider.PostgreSQL => provider => new PostgresDataContext(
                    connectionString,
                    provider.GetRequiredService<IDataJSONServices>()
                // TODO: Resolve other dependencies from provider here
                ),
                // DatabaseProvider.MySQL => _ => new MySqlDataContext(connectionString),
                //  DatabaseProvider.SqlServer => _ => new MsSqlDataContext(connectionString),
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
