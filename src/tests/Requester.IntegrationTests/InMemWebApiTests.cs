using System;
using System.Net;
using FluentAssertions;
using Microsoft.Owin.Testing;
using Requester.TestWebApi;
using Requester.TestWebApi.Model;
using Requester.Validation;
using Xunit;

namespace Requester.IntegrationTests
{
    public class InMemWebApiTests : IDisposable
    {
        private TestServer _server;

        public InMemWebApiTests()
        {
            _server = TestServer.Create<Startup>();
            When.MessageHandlerFn = () => _server.Handler;
        }

        public void Dispose()
        {
            When.MessageHandlerFn = null;

            _server?.Dispose();
            _server = null;
        }

        [Fact]
        public void Can_use_the_When_construct()
        {
            var person = new Person
            {
                Id = Guid.NewGuid(),
                FirstName = "Daniel",
                LastName = "Wertheim",
                Age = 35
            };

            When.PutAsJson($"{_server.BaseAddress}/api/persons/", person)
                .TheResponse(should => should
                    .BeSuccessful()
                    .HaveStatus(HttpStatusCode.Created));

            When.GetOfJson(_server.BaseAddress + "api/persons/" + person.Id)
                .TheResponse(should => should
                    .BeSuccessful()
                    .BeJsonResponse()
                    .HaveSpecificValue("FirstName", "Daniel"));
        }

        [Fact]
        public async void Can_use_the_HttpRequester()
        {
            var person = new Person
            {
                Id = Guid.NewGuid(),
                FirstName = "Daniel",
                LastName = "Wertheim",
                Age = 35
            };

            using (var requester = new HttpRequester($"{_server.BaseAddress}/api/persons/", _server.Handler))
            {
                var forThePut = await requester.PutEntityAsync(person);
                forThePut.TheResponse(should => should
                    .BeSuccessful()
                    .HaveStatus(HttpStatusCode.Created));

                var forTheGet = await requester.GetAsync(person.Id.ToString());
                forTheGet.TheResponse(should => should
                    .BeSuccessful()
                    .BeJsonResponse()
                    .HaveSpecificValue("FirstName", "Daniel"));

                var getResponse = await requester.GetAsync<Person>(person.Id.ToString());
                var retrieved = getResponse.Content;
                retrieved.Should().NotBeNull();
                retrieved.FirstName.Should().Be("Daniel");
            }
        }
    }
}