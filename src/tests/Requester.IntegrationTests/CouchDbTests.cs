using System;
using System.Net;
using FluentAssertions;
using Newtonsoft.Json;
using Requester.Validation;
using Xunit;

namespace Requester.IntegrationTests
{
    public class CouchDbTests : IDisposable
    {
        private const string U = "dev";
        private const string P = "1q2w3e4r";
        private const string DbUrl = "http://192.168.2.20:5984/mydb/";
        private static readonly string DbUrlWithCredentials = $"http://{U}:{P}@192.168.2.20:5984/mydb/";
        private readonly HttpRequester _dbRequester;

        public CouchDbTests()
        {
            _dbRequester = HttpRequester.Create(DbUrlWithCredentials);
            var head = _dbRequester.HeadAsync().Result;
            if(head.IsSuccess)
                _dbRequester.DeleteAsync().Wait();
        }

        public void Dispose()
        {
            _dbRequester.DeleteAsync().Wait();
            _dbRequester.Dispose();
        }

        [Fact]
        public void Can_use_the_When_construct()
        {
            When.Put(DbUrlWithCredentials)
                .TheResponse(should => should.BeSuccessful());

            When.Head(DbUrlWithCredentials)
                .TheResponse(should => should.BeSuccessful());

            When.PostOfJson(DbUrlWithCredentials, "{\"_id\":\"doc1\", \"name\": \"Daniel Wertheim\", \"address\":{\"street\":\"One way\", \"zip\":12345}, \"hobbies\":[\"programming\",\"running\"]}")
                .TheResponse(should => should
                    .BeSuccessful()
                    .HaveStatus(HttpStatusCode.Created));

            When.PutOfJson(DbUrlWithCredentials + "doc2", "{\"name\": \"John Doe\", \"address\":{\"street\":\"Two way\", \"zip\":54321}, \"hobbies\":[\"programming\",\"running\"]}")
                .TheResponse(should => should
                    .BeSuccessful()
                    .HaveStatus(HttpStatusCode.Created));

            When.GetOfJson(DbUrlWithCredentials + "doc1")
                .TheResponse(should => should
                    .BeSuccessful()
                    .BeJsonResponse()
                    .HaveJsonConformingToSchema(@"{
                        _id: {type: 'string', required: true},
                        _rev: {type: 'string', required: true},
                        name: {type: 'string'},
                        address: {type: 'object', properties: {zip: {type: 'integer'}}},
                        hobbies: {type: 'array', items: {type: 'string'}}
                    }")
                    .Match(new { _id = "doc1", name = "Daniel Wertheim" }));

            When.GetOfJson(DbUrlWithCredentials + "doc2")
                .TheResponse(should => should
                    .BeSuccessful()
                    .BeJsonResponse()
                    .HaveSpecificValueFor("_id", "doc2")
                    .HaveSpecificValueFor("hobbies[0]", "programming")
                    .HaveSpecificValueFor("address.zip", 54321));

            var doc1 = When.Head(DbUrlWithCredentials + "doc1").TheResponse(should => should.BeSuccessful());

            When.Delete(DbUrlWithCredentials + "doc1?rev=" + doc1.ETag).TheResponse(should => should.BeSuccessful());
        }

        [Fact]
        public void Can_use_the_HttpRequester()
        {
            var putDbResponse = _dbRequester.PutAsync().Result;
            putDbResponse
                .TheResponse(should => should.BeSuccessful());

            var headDbResponse = _dbRequester.HeadAsync().Result;
            headDbResponse
                .TheResponse(should => should.BeSuccessful());

            var postDocResponse = _dbRequester
                .PostJsonAsync("{\"_id\":\"doc1\", \"name\": \"Daniel Wertheim\", \"address\":{\"street\":\"One way\", \"zip\":12345}, \"hobbies\":[\"programming\",\"running\"]}")
                .Result;
            postDocResponse
                .TheResponse(should => should
                    .BeSuccessful()
                    .HaveStatus(HttpStatusCode.Created));

            var postEntityResponse = _dbRequester
                .PostEntityAsJsonAsync(new
                {
                    _id = "ent1",
                    Name = "Daniel Wertheim",
                    Address = new
                    {
                        Street = "One way",
                        Zip = 12345
                    },
                    Hobbies = new[] { "programming", "running" }
                })
                .Result;
            postEntityResponse
                .TheResponse(should => should
                    .BeSuccessful()
                    .HaveStatus(HttpStatusCode.Created));

            var putDocResponse = _dbRequester
                .PutJsonAsync("{\"name\": \"John Doe\", \"address\":{\"street\":\"Two way\", \"zip\":54321}, \"hobbies\":[\"programming\",\"running\"]}", "/doc2")
                .Result;
            putDocResponse
                .TheResponse(should => should
                    .BeSuccessful()
                    .HaveStatus(HttpStatusCode.Created));

            var putEntityResponse = _dbRequester
                .PutEntityAsJsonAsync(new
                {
                    Name = "John Doe",
                    Address = new
                    {
                        Street = "One way",
                        Zip = 54321
                    },
                    Hobbies = new[] { "programming", "running" }
                }, "/ent2")
                .Result;
            putEntityResponse
                .TheResponse(should => should
                    .BeSuccessful()
                    .HaveStatus(HttpStatusCode.Created));

            var getDoc1Response = _dbRequester.GetAsync("/doc1").Result;
            getDoc1Response
                .TheResponse(should => should
                    .BeSuccessful()
                    .BeJsonResponse()
                    .HaveJsonConformingToSchema(@"{
                        _id: {type: 'string', required: true},
                        _rev: {type: 'string', required: true},
                        name: {type: 'string'},
                        address: {type: 'object', properties: {zip: {type: 'integer'}}},
                        hobbies: {type: 'array', items: {type: 'string'}}
                    }")
                    .Match(new { _id = "doc1", name = "Daniel Wertheim" }));

            var getEnt1Response = _dbRequester.GetAsync("/ent1").Result;
            getEnt1Response
                .TheResponse(should => should
                    .BeSuccessful()
                    .BeJsonResponse()
                    .HaveJsonConformingToSchema(@"{
                        _id: {type: 'string', required: true},
                        _rev: {type: 'string', required: true},
                        name: {type: 'string'},
                        address: {type: 'object', properties: {zip: {type: 'integer'}}},
                        hobbies: {type: 'array', items: {type: 'string'}}
                    }")
                    .Match(new { _id = "ent1", name = "Daniel Wertheim" }));

            var getDoc2Response = _dbRequester.GetAsync("/doc2").Result;
            getDoc2Response
                .TheResponse(should => should
                    .BeSuccessful()
                    .BeJsonResponse()
                    .HaveSpecificValueFor("_id", "doc2")
                    .HaveSpecificValueFor("hobbies[0]", "programming")
                    .HaveSpecificValueFor("address.zip", 54321));

            var getEnt2Response = _dbRequester.GetAsync("/ent2").Result;
            getEnt2Response
                .TheResponse(should => should
                    .BeSuccessful()
                    .BeJsonResponse()
                    .HaveSpecificValueFor("_id", "ent2")
                    .HaveSpecificValueFor("hobbies[0]", "programming")
                    .HaveSpecificValueFor("address.zip", 54321));

            var getEnt1AsEntityResponse = _dbRequester.GetAsync<Person>("/ent1").Result;
            getEnt1AsEntityResponse.IsSuccess.Should().BeTrue();
            getEnt1AsEntityResponse.Content.Name.Should().Be("Daniel Wertheim");
            getEnt1AsEntityResponse.Content.Address.Zip.Should().Be(12345);
            getEnt1AsEntityResponse.Content.Hobbies.Should().ContainInOrder("programming", "running");

            var headDoc1Response = _dbRequester.HeadAsync("/doc1").Result;
            headDoc1Response.TheResponse(should => should.BeSuccessful());

            var deleteDoc1Response = _dbRequester.DeleteAsync($"/doc1?rev={headDoc1Response.ETag}").Result;
            deleteDoc1Response.TheResponse(should => should.BeSuccessful());
        }

        [Fact]
        public async void Can_use_basic_auth_on_HttpRequester()
        {
            using (var requester = HttpRequester.Create(DbUrl).Configure(cfg => cfg.WithBasicAuthorization(U,P)))
            {
                var db = await requester.PutAsync();
                db.TheResponse(should => should.BeSuccessful());

                var newDoc = await requester.PostJsonAsync("{\"value\":\"can_use_basic_auth_on_HttpRequester\"}");
                newDoc.TheResponse(should => should.BeSuccessful());

                var deleteDoc = await requester.DeleteAsync(newDoc.Location.Replace(requester.Config.BaseAddress.ToString(), string.Empty) + "?rev=" + newDoc.ETag);
                deleteDoc.TheResponse(should => should.BeSuccessful());
            }
        }

        [Fact]
        public async void Can_have_resulting_entity_with_Post_HttpRequester()
        {
            using (var requester = HttpRequester.Create(DbUrl).Configure(cfg => cfg.WithBasicAuthorization(U, P)))
            {
                var db = await requester.PutAsync();
                db.TheResponse(should => should.BeSuccessful());

                var newDoc = await requester.PostJsonAsync<dynamic>("{\"value\":\"one\"}");
                newDoc.TheResponse(should => should.BeSuccessful());

                string id = newDoc.Content.id;

                var putDoc = await requester.PutJsonAsync<dynamic>("{\"value\":\"two\"}", $"{id}?rev={newDoc.ETag}");
                putDoc.TheResponse(should => should.BeSuccessful());
            }
        }

        private class Person
        {
            [JsonProperty("_id")]
            public string Id { get; set; }
            public string Name { get; set; }
            public Address Address { get; set; }
            public string[] Hobbies { get; set; }
        }

        private class Address
        {
            public string Street { get; set; }
            public int Zip { get; set; }
        }
    }
}