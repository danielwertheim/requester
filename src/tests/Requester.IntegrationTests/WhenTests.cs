using System;
using System.Net;
using Microsoft.Owin.Testing;
using Requester.TestWebApi;
using Requester.TestWebApi.Model;
using Requester.Validation;
using Xunit;

namespace Requester.IntegrationTests
{
    public class WhenTests : IDisposable
    {
        private TestServer _server;

        public WhenTests()
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
        public void Flow_tests()
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

            When.GetOfJson($"{_server.BaseAddress}/api/persons/{person.Id}")
                .TheResponse(should => should
                    .BeSuccessful()
                    .BeJsonResponse()
                    .HaveSpecificValueFor("firstName", "Daniel"));

            person.Age = 42;
            When.PutAsJson($"{_server.BaseAddress}/api/persons/", person)
                .TheResponse(should => should
                    .BeSuccessful()
                    .HaveStatus(HttpStatusCode.OK));

            When.Delete($"{_server.BaseAddress}/api/persons/{person.Id}")
                .TheResponse(should => should
                    .BeSuccessful());

            When.PostAsJson($"{_server.BaseAddress}/api/persons/", person)
                .TheResponse(should => should
                    .BeSuccessful()
                    .HaveStatus(HttpStatusCode.Created)
                    .BeJsonResponse()
                    .NotHavingSpecificValueFor("id", person.Id));
        }
    }
}