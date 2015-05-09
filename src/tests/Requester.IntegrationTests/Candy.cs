using System.Net;
using System.Net.Http;
using NUnit.Framework;
using Requester.Http;
using Requester.Validation;

namespace Requester.IntegrationTests
{
    [TestFixture(Description = "Little piece of candy shown, running against a CouchDB node")]
    public class Candy
    {
        private const string DbUrl = "http://sa:test@ci01:5984/mydb/";
        private HttpRequester _requester;

        [TestFixtureSetUp]
        public void Setup()
        {
            _requester = new HttpRequester(DbUrl);
            _requester.SendAsync(new HttpRequest(HttpMethod.Delete)).Wait();
        }

        [TestFixtureTearDown]
        public void Clean()
        {
            _requester.SendAsync(new HttpRequest(HttpMethod.Delete)).Wait();
            _requester.Dispose();
        }

        [Test]
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

            When.PutOfJson(DbUrl + "doc2", "{\"name\": \"Daniel Wertheim\", \"address\":{\"street\":\"Two way\", \"zip\":54321}, \"hobbies\":[\"programming\",\"running\"]}")
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
    }
}