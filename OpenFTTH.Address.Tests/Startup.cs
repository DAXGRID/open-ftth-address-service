using Microsoft.Extensions.DependencyInjection;
using OpenFTTH.Address.Business;
using OpenFTTH.Address.Business.Repository;
using OpenFTTH.Address.Tests.TestData;
using OpenFTTH.CQRS;
using System;
using System.Reflection;

namespace OpenFTTH.Address.Tests
{
    public class Startup
    {
        private static string _connectionString = Environment.GetEnvironmentVariable("test_address_store_connection");

        public void ConfigureServices(IServiceCollection services)
        {
            // Test against in-memory repository if no connection string env is set
            if (_connectionString == null)
            {
                services.AddSingleton<IAddressRepository>(x =>
                    new InMemAddressRepository(TestAddressData.AccessAddresses)
                );
            }
            // Otherwise test against Postgres database (that must contain Danish addresses or the tests will fail)
            else
            {
                services.AddSingleton<IAddressRepository>(x =>
                    new PostgresAddressRepository(_connectionString)
                );
            }

            services.AddSingleton<IQueryDispatcher, QueryDispatcher>();
            services.AddSingleton<ICommandDispatcher, CommandDispatcher>();

            var businessAssemblies = new Assembly[] { AppDomain.CurrentDomain.Load("OpenFTTH.Address.Business") };

            services.AddCQRS(businessAssemblies);
        }
    }
}
