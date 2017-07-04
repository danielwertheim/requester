using System;
using System.Net;
using FakeWebApi;
using FakeWebApi.Model;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Requester.Validation;
using Xunit;

namespace IntegrationTests
{
    public class WhenTests : IDisposable
    {
        private TestServer _server;

        public WhenTests()
        {
            var builder = CreateWebHostBuilder();

            _server = new TestServer(builder);

            When.MessageHandlerFn = () => _server.CreateHandler();
        }

        private static IWebHostBuilder CreateWebHostBuilder()
        {
            var config = new ConfigurationBuilder().Build();

            var host = new WebHostBuilder()
                .UseConfiguration(config)
                .UseStartup<Startup>();

            return host;
        }

        public void Dispose()
        {
            When.MessageHandlerFn = null;

            _server?.Dispose();
            _server = null;
        }

        [Fact]
        public void Flow_tests()
        {
            var person = new Person
            {
                Id = Guid.NewGuid(),
                FirstName = "Daniel",
                LastName = "Wertheim",
                Age = 35
            };

            When.PutAsJson($"{_server.BaseAddress}api/persons/", person)
                .TheResponse(should => should
                    .BeSuccessful()
                    .HaveStatus(HttpStatusCode.Created));

            When.GetOfJson($"{_server.BaseAddress}api/persons/{person.Id}")
                .TheResponse(should => should
                    .BeSuccessful()
                    .BeJsonResponse()
                    .HaveSpecificValueFor("firstName", "Daniel"));

            person.Age = 42;
            When.PutAsJson($"{_server.BaseAddress}api/persons/", person)
                .TheResponse(should => should
                    .BeSuccessful()
                    .HaveStatus(HttpStatusCode.OK));

            When.Delete($"{_server.BaseAddress}api/persons/{person.Id}")
                .TheResponse(should => should
                    .BeSuccessful());

            When.PostAsJson($"{_server.BaseAddress}api/persons/", person)
                .TheResponse(should => should
                    .BeSuccessful()
                    .HaveStatus(HttpStatusCode.Created)
                    .BeJsonResponse()
                    .NotHavingSpecificValueFor("id", person.Id));
        }
    }
}