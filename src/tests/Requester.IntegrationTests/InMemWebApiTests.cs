using System;
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
            _server.BaseAddress = new Uri("http://ce73f0a3bc6f476995dd88dc14f60066");
            When.MessageHandlerFn = () => _server.Handler;
        }

        public void Dispose()
        {
            When.MessageHandlerFn = null;

            _server?.Dispose();
            _server = null;
        }

        [Fact]
        public void Can_use_When_with_message_handler_to_access_in_mem_wepapi()
        {
            var person = new Person
            {
                Id = Guid.NewGuid(),
                FirstName = "Daniel",
                LastName = "Wertheim",
                Age = 35
            };

            When.PutAsJson(_server.BaseAddress + "api/persons", person)
                .TheResponse(should => should.BeSuccessful());

            When.GetOfJson(_server.BaseAddress + "api/persons/" + person.Id)
                .TheResponse(should => should
                    .BeSuccessful()
                    .BeJsonResponse()
                    .HaveSpecificValue("FirstName", "Daniel"));
        }

        [Fact]
        public async void Can_use_Requester_with_message_handler_to_access_in_mem_wepapi()
        {
            var person = new Person
            {
                Id = Guid.NewGuid(),
                FirstName = "Daniel",
                LastName = "Wertheim",
                Age = 35
            };

            using (var requester = new HttpRequester(_server.BaseAddress + "/api/persons/", _server.Handler))
            {
                var forPut = await requester.PutEntityAsync(person);
                forPut.TheResponse(should => should.BeSuccessful());

                var forGet = await requester.GetAsync(person.Id.ToString());
                forGet.TheResponse(should => should
                    .BeSuccessful()
                    .BeJsonResponse()
                    .HaveSpecificValue("FirstName", "Daniel"));

                var retrievedAsPerson = (await requester.GetAsync<Person>(person.Id.ToString())).Content;
                retrievedAsPerson.Should().NotBeNull();
                retrievedAsPerson.FirstName.Should().Be("Daniel");
            }
        }
    }
}