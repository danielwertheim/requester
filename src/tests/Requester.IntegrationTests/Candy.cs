using System;
using System.Net;
using System.Runtime.InteropServices;
using Requester.Validation;
using Xunit;

namespace Requester.IntegrationTests
{
    public class Candy : IDisposable
    {
        private const string DbUrl = "http://developer:1q2w3e4r@development:5984/mydb/";
        private readonly HttpRequester _dbRequester;

        public Candy()
        {
            _dbRequester = new HttpRequester(DbUrl);
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
                    .Match(new {_id = "doc1", name = "Daniel Wertheim"}));

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
            var putDbResponse =_dbRequester.PutAsync().Result;
            putDbResponse
                .TheResponse(should => should.BeSuccessful());

            var headDbResponse = _dbRequester.HeadAsync().Result;
            headDbResponse
                .TheResponse(should => should.BeSuccessful());

            var postDocResponse = _dbRequester
                .PostAsync("{\"_id\":\"doc1\", \"name\": \"Daniel Wertheim\", \"address\":{\"street\":\"One way\", \"zip\":12345}, \"hobbies\":[\"programming\",\"running\"]}")
                .Result;
            postDocResponse
                .TheResponse(should => should
                    .BeSuccessful()
                    .HaveStatus(HttpStatusCode.Created));

            var putDocResponse = _dbRequester
                .PutAsync("{\"name\": \"John Doe\", \"address\":{\"street\":\"Two way\", \"zip\":54321}, \"hobbies\":[\"programming\",\"running\"]}", "/doc2")
                .Result;
            putDocResponse
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

            var getDoc2Response = _dbRequester.GetAsync("doc2").Result;
            getDoc2Response
                .TheResponse(should => should
                    .BeSuccessful()
                    .BeJsonResponse()
                    .HaveSpecificValue("_id", "doc2")
                    .HaveSpecificValue("hobbies[0]", "programming")
                    .HaveSpecificValue("address.zip", 54321));

            var headDoc1Response = _dbRequester.HeadAsync("/doc1").Result;
            headDoc1Response.TheResponse(should => should.BeSuccessful());

            var deleteDoc1Response = _dbRequester.DeleteAsync($"/doc1?rev={headDoc1Response.ETag}").Result;
            deleteDoc1Response.TheResponse(should => should.BeSuccessful());
        }
    }
}