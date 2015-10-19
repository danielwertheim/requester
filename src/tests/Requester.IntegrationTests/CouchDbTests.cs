using System;
using System.Net;
using FluentAssertions;
using Newtonsoft.Json;
using Requester.Validation;
using Xunit;

namespace Requester.IntegrationTests
{
    [Collection("CouchDB tests")]
    public class CouchDbTests : IDisposable
    {
        private const string DbUrl = "http://developer:1q2w3e4r@development:5984/mydb/";
        private readonly HttpRequester _dbRequester;

        public CouchDbTests()
        {
            _dbRequester = new HttpRequester(DbUrl);
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
        public void Can_eat_candy_like_a_monster()
        {
            When.Put(DbUrl)
                .TheResponse(should => should.BeSuccessful());

            When.Head(DbUrl)
                .TheResponse(should => should.BeSuccessful());

            When.PostOfJson(DbUrl, "{\"_id\":\"doc1\", \"name\": \"Daniel Wertheim\", \"address\":{\"street\":\"One way\", \"zip\":12345}, \"hobbies\":[\"programming\",\"running\"]}")
                .TheResponse(should => should
                    .BeSuccessful()
                    .HaveStatus(HttpStatusCode.Created));

            When.PutOfJson(DbUrl + "doc2", "{\"name\": \"John Doe\", \"address\":{\"street\":\"Two way\", \"zip\":54321}, \"hobbies\":[\"programming\",\"running\"]}")
                .TheResponse(should => should
                    .BeSuccessful()
                    .HaveStatus(HttpStatusCode.Created));

            When.GetOfJson(DbUrl + "doc1")
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

            When.GetOfJson(DbUrl + "doc2")
                .TheResponse(should => should
                    .BeSuccessful()
                    .BeJsonResponse()
                    .HaveSpecificValue("_id", "doc2")
                    .HaveSpecificValue("hobbies[0]", "programming")
                    .HaveSpecificValue("address.zip", 54321));

            var doc1 = When.Head(DbUrl + "doc1").TheResponse(should => should.BeSuccessful());

            When.Delete(DbUrl + "doc1?rev=" + doc1.ETag).TheResponse(should => should.BeSuccessful());
        }

        [Fact]
        public void Can_drink_soda_like_a_foo()
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
                .PostEntityAsync(new { _id = "ent1", Name = "Daniel Wertheim", Address = new { Street = "One way", Zip = 12345 }, Hobbies = new[] { "programming", "running" } })
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
                .PutEntityAsync(new { Name = "John Doe", Address = new { Street = "One way", Zip = 54321 }, Hobbies = new[] { "programming", "running" } }, "/ent2")
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
                    .HaveSpecificValue("_id", "doc2")
                    .HaveSpecificValue("hobbies[0]", "programming")
                    .HaveSpecificValue("address.zip", 54321));

            var getEnt2Response = _dbRequester.GetAsync("/ent2").Result;
            getEnt2Response
                .TheResponse(should => should
                    .BeSuccessful()
                    .BeJsonResponse()
                    .HaveSpecificValue("_id", "ent2")
                    .HaveSpecificValue("hobbies[0]", "programming")
                    .HaveSpecificValue("address.zip", 54321));

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