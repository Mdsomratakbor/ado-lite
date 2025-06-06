using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AdoLite.Core.Enums;
using AdoLite.Core.Interfaces;
using AdoLite.Core.Services;
using AdoLite.Extension;
using AdoLite.MySql;
using AdoLite.Postgres;
using AdoLite.SqlServer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using Xunit;

namespace AdoLite.Tests.Extensions
{
    public class DataContextServiceCollectionExtensionsTests
    {
        private const string TestConnectionString = "Server=.;Database=TestDb;User Id=sa;Password=007;TrustServerCertificate=True;";

        [Fact]
        public void ShouldThrowForUnsupportedProvider()
        {
            // Arrange
            var services = new ServiceCollection();
            var unsupportedProvider = (DatabaseProvider)999; // Invalid enum

            // Act & Assert
            var exception = Assert.Throws<NotSupportedException>(() =>
                services.AddAdoLiteDataContext(unsupportedProvider, TestConnectionString));

            Assert.Contains("not supported", exception.Message);
        }
        [Fact]
        public void ShouldThrowWhenIDataJSONServicesIsMissingInManualResolution()
        {
            // Arrange
            var services = new ServiceCollection();

            // Manually register context without required IDataJSONServices
            services.Add(new ServiceDescriptor(typeof(IDataContext), provider =>
                new MsSqlDataContext(TestConnectionString, null!), ServiceLifetime.Scoped));

            var provider = services.BuildServiceProvider();

            // Act & Assert
            var ex = Assert.Throws<ArgumentNullException>(() => provider.GetRequiredService<IDataContext>());
            Assert.NotNull(ex);
            Assert.Equal("jsonServices", ex.ParamName); // Optional: check parameter name
        }

        [Fact]
        public void ShouldThrowWhenIDataJSONServicesNotRegistered()
        {
            // Arrange
            var services = new ServiceCollection();

            // Register context via DI factory, but don't register IDataJSONServices
            services.Add(new ServiceDescriptor(typeof(IDataContext), provider =>
                new MsSqlDataContext(TestConnectionString, provider.GetRequiredService<IDataJSONServices>()), ServiceLifetime.Scoped));

            var provider = services.BuildServiceProvider();

            // Act & Assert
            var ex = Assert.Throws<InvalidOperationException>(() => provider.GetRequiredService<IDataContext>());
            Assert.NotNull(ex);
        }

        [Fact]
        public void ShouldThrowIfIDataContextIsNotRegistered()
        {
            // Arrange
            var services = new ServiceCollection(); // Missing any registrations
            var provider = services.BuildServiceProvider();

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() =>
                provider.GetRequiredService<IDataContext>());
        }


        [Theory]
       // [InlineData(DatabaseProvider.PostgreSQL, typeof(PostgresDataContext))]
        [InlineData(DatabaseProvider.SqlServer, typeof(MsSqlDataContext))]
        //[InlineData(DatabaseProvider.MySQL, typeof(MySqlDataContext))]
        public void ShouldRegisterAndResolveCorrectDataContext(DatabaseProvider providerType, Type expectedType)
        {
            // Arrange
            var services = new ServiceCollection();

           // var mockJsonService = new Mock<IDataJSONServices>();
           // services.AddSingleton(mockJsonService.Object);
            services.AddAdoLiteDataContext(providerType, TestConnectionString);
            // Act
            var serviceProvider = services.BuildServiceProvider();
            var dataContext = serviceProvider.GetRequiredService<IDataContext>();

            // Assert
            Assert.NotNull(dataContext);
            Assert.IsType(expectedType, dataContext);
        }

        [Fact]
        public void ShouldRegisterAndResolveIDataJSONServices()
        {
            // Arrange
            var services = new ServiceCollection();
            services.UseSqlServer(TestConnectionString);

            // Act
            var serviceProvider = services.BuildServiceProvider();
            var jsonService = serviceProvider.GetRequiredService<IDataJSONServices>();

            // Assert
            Assert.NotNull(jsonService);
            Assert.IsAssignableFrom<IDataJSONServices>(jsonService);
        }


        [Fact]
        public void UsePostgreSQL_ShouldRegisterPostgresDataContext()
        {
            var services = new ServiceCollection();
            services.UsePostgreSQL(TestConnectionString);

            var provider = services.BuildServiceProvider();
            var context = provider.GetRequiredService<IDataContext>();

            Assert.IsType<PostgresDataContext>(context);
        }

        [Fact]
        public void UseSqlServer_ShouldRegisterMsSqlDataContext()
        {
            var services = new ServiceCollection();
            services.UseSqlServer(TestConnectionString);

            var provider = services.BuildServiceProvider();
            var context = provider.GetRequiredService<IDataContext>();

            Assert.IsType<MsSqlDataContext>(context);
        }

        [Fact]
        public void UseMySQL_ShouldRegisterMySqlDataContext()
        {
            var services = new ServiceCollection();
            services.UseMySQL(TestConnectionString);

            var provider = services.BuildServiceProvider();
            var context = provider.GetRequiredService<IDataContext>();

            Assert.IsType<MySqlDataContext>(context);
        }

    }
}
