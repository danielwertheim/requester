using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using FluentAssertions;
using Microsoft.Owin.Testing;
using Requester.FakeWebApi;
using Requester.FakeWebApi.Model;
using Requester.Validation;
using Xunit;

namespace Requester.IntegrationTests
{
    public class HttpRequesterTests : IDisposable
    {
        private TestServer _server;

        public HttpRequesterTests()
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
        public async void Can_retrieve_no_content()
        {
            using (var requester = new HttpRequester($"{_server.BaseAddress}/api/test/nocontent", _server.Handler))
            {
                var request = new HttpRequest(HttpMethod.Get);
                request.WithAccept(ct => ct.ApplicationJson);

                var response = await requester.SendAsync(request);
                response.TheResponse(should => should
                    .BeSuccessful());

                var entityResponse = await requester.GetAsync<Person>();
                entityResponse.TheResponse(should => should
                    .BeSuccessful());
                entityResponse.Content.Should().BeNull();
            }
        }

        [Fact]
        public async void Can_retrieve_null_content()
        {
            using (var requester = new HttpRequester($"{_server.BaseAddress}/api/test/null", _server.Handler))
            {
                var request = new HttpRequest(HttpMethod.Get);
                request.WithAccept(ct => ct.ApplicationJson);

                var textResponse = await requester.SendAsync(request);
                textResponse.TheResponse(should => should
                    .BeSuccessful());

                var entityResponse = await requester.GetAsync<Person>();
                entityResponse.TheResponse(should => should
                    .BeSuccessful());
                entityResponse.Content.Should().BeNull();
            }
        }

        [Fact]
        public async void Can_retrieve_empty_content()
        {
            using (var requester = new HttpRequester($"{_server.BaseAddress}/api/test/empty", _server.Handler))
            {
                var request = new HttpRequest(HttpMethod.Get);
                request.WithAccept(ct => ct.ApplicationJson);

                var textResponse = await requester.SendAsync(request);
                textResponse.TheResponse(should => should
                    .BeSuccessful());

                var entityResponse = await requester.GetAsync<Person>();
                entityResponse.TheResponse(should => should
                    .BeSuccessful());
                entityResponse.Content.Should().BeNull();
            }
        }

        [Fact]
        public async void Can_POST_form_url_encoded_content()
        {
            using (var requester = new HttpRequester($"{_server.BaseAddress}/api/relay/", _server.Handler))
            {
                var request = new HttpRequest(HttpMethod.Post);
                request.WithContent(new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    { "key", "test" },
                    { "value", "42" }
                }));

                var response = await requester.SendAsync(request);
                response.TheResponse(should => should
                    .BeSuccessful()
                    .BeJsonResponse()
                    .HaveSpecificValueFor("key", "test")
                    .HaveSpecificValueFor("value", "42"));
            }
        }

        [Fact]
        public async void Can_PUT_form_url_encoded_content()
        {
            using (var requester = new HttpRequester($"{_server.BaseAddress}/api/relay/", _server.Handler))
            {
                var request = new HttpRequest(HttpMethod.Put);
                request.WithContent(new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    { "key", "test" },
                    { "value", "42" }
                }));

                var response = await requester.SendAsync(request);
                response.TheResponse(should => should
                    .BeSuccessful()
                    .BeJsonResponse()
                    .HaveSpecificValueFor("key", "test")
                    .HaveSpecificValueFor("value", "42"));
            }
        }

        [Fact]
        public async void Flow_tests()
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
                var forTheCreatingPut = await requester.PutEntityAsJsonAsync(person);
                forTheCreatingPut.TheResponse(should => should
                    .BeSuccessful()
                    .HaveStatus(HttpStatusCode.Created));

                var forTheGet = await requester.GetAsync(person.Id.ToString());
                forTheGet.TheResponse(should => should
                    .BeSuccessful()
                    .BeJsonResponse()
                    .HaveSpecificValueFor("firstName", "Daniel"));

                person.Age = 42;
                var forTheUpdatingPut = await requester.PutEntityAsJsonAsync(person);
                forTheUpdatingPut.TheResponse(should => should
                    .BeSuccessful()
                    .HaveStatus(HttpStatusCode.OK));

                var forTheDelete = await requester.DeleteAsync(person.Id.ToString());
                forTheDelete.TheResponse(should => should
                    .BeSuccessful()
                    .HaveStatus(HttpStatusCode.OK));

                var forTheCreatingPost = await requester.PostEntityAsJsonAsync(person);
                forTheCreatingPost.TheResponse(should => should
                    .BeSuccessful()
                    .HaveStatus(HttpStatusCode.Created)
                    .BeJsonResponse()
                    .NotHavingSpecificValueFor("id", person.Id));
            }
        }
    }
}