# Requester
Requester is a Http-request fiddling magical something that is designed to help me, and maybe you as well, to validate APIs. It's mainly focused on APIs that work with JSON. It was put together after some fiddling with an awesome NodeJS peer: [FrisbyJS](http://frisbyjs.com/ "FrisbyJS").

## Disclaimer, Remarks...
This is still an early release hence stuff WILL change. Especially additions for URL-building and helpers for doing requests that should not be validated.

## NuGet
[Of course](https://www.nuget.org/packages/requester). Just do:

	install-package requester

Then import the namespaces:

```csharp
using Requester;
using Requester.Validation;
```

## Samples - Validation
The samples below are written to work against a local CouchDB installation.

```csharp
When.Put("http://localhost:5984/mydb")
    .TheResponse(should => should.BeSuccessful());

When.Head("http://localhost:5984/mydb")
    .TheResponse(should => should.BeSuccessful());
```

### Authentication
For now only basic authentication is supported and it can be defined in two ways: via URL or using `WithAuthentication`.

```csharp
When.Put("http://foo:bar@localhost:5984/mydb")
	.TheResponse(should => should.BeSuccessful());

//vs

When.Put(DbUrl, cfg => cfg
    .WithBasicAuthorization("foo", "bar"))
    .TheResponse(should => should.BeSuccessful());
```

**Please note**, that when passing credentials in the URL, you need to encode your values.

### Custom headers
```csharp
When.Put(DbUrl, cfg => cfg
    .WithHeader("foo", "bar"))
	.WithHeader(h => h.Accept, "application/json"))
    .TheResponse(should => should.BeSuccessful());
```

### More validation expectations()

```csharp
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

        When.Put(DbUrl, cfg => cfg
            .WithHeader(h => h.Accept, "application/json"))
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
```

### Custom testing framework assertion exceptions
To keep down some dependencies, Requester throws a custom `RequesterAssertionException` but if you want your specific testing framework exceptions, just hook them in:

```csharp
AssertionExceptionFactory.ExceptionFn = msg => new NUnit.Framework.AssertionException(msg);
```


