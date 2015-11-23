# Requester
**Requester:**

[![Nuget](https://img.shields.io/nuget/v/requester.svg)](https://www.nuget.org/packages/requester/) [![Users](https://img.shields.io/nuget/dt/requester.svg)](https://www.nuget.org/packages/requester/)

**Requester.Validation:**

[![Nuget](https://img.shields.io/nuget/v/requester.validation.svg)](https://www.nuget.org/packages/requester.validation/) [![Users](https://img.shields.io/nuget/dt/requester.validation.svg)](https://www.nuget.org/packages/requester.validation/)

Requester is designed to help you interact with, as well as validate, web APIs. It's mainly focused on APIs that work with JSON.

It was put together after some fiddling with an awesome NodeJS peer: [FrisbyJS](http://frisbyjs.com/ "FrisbyJS"), for validationg web APIs.

Most of the `HttpRequester` has been extracted from `MyCouch` [the async CouchDB client for .Net](https://github.com/danielwertheim/mycouch).

## NuGet
It supports .Net4.5+ and there are **two packages**: one for *web API interaction*; one for *web API validation*.

### Requester NuGet
The [Requester package](https://www.nuget.org/packages/requester), is only for consuming web APIs. For validation features: see [Requester.Validation](https://www.nuget.org/packages/requester.validation)

```csharp
using Requester;
```

```csharp
//Sample of interacting with a CouchDB instance
using(var requester = new HttpRequester("http://localhost:5984/mydb"))
{
  //Ensure db is created
  await requester.SendAsync(new HttpRequest(HttpMethod.Put));

  //Create a document
  await requester.SendAsync(
    new HttpRequest(HttpMethod.Put, "/mydocid").WithJsonContent(someJson));

  //Our using simplified overloads...
  await requester.PostJsonAsync(json);
  await requester.PostEntityAsJsonAsync(new { _id = "doc2" Name = "Foo" });

  await requester.PutJsonAsync(json, "/doc3");
  await requester.PutEntityAsJsonAsync(new { Name = "Foo" }, "/doc4");

  var jsonResponse = await requester.GetAsync("/doc3");
  var entityResponse = await requester.GetAsync<MyDoc>("/doc3");

  var deleteResponse = await requester.DeleteAsync($"/doc3?rev={jsonResponse.ETag}");
}
```

The response of the `HttpRequester` is a `HttpResponse`.

```csharp
//Everything is a response (HEAD, GET, POST, PUT, DELETE)
var response = await requester.SendAsync(new HttpRequest(HttpMethod.Get, "/mydocid"));

//The resonse has like: StatusCode, Reason, Content, ETag, ContentType etc.
Debug.WriteLine(response.ToStringDebugVersion(includeContent: true));
```

The output would be:

```
RequestUri: http://localhost:5984/mydb/doc1
RequestMethod: GET
Status: OK(200)
Reason: OK
ETag: 1-23c4402e369a7a56059d912e53d320ee
ContentType:application/json
HasContent:True
Content:{
    "_id":"doc1",
    "_rev":"1-23c4402e369a7a56059d912e53d320ee",
    "name":"Daniel Wertheim",
    "address":{"street":"One way","zip":12345},
    "hobbies":["programming","running"]}
```

#### Configuration
You can configure it e.g. at contruction to via `Configure()` or via `Config`.

```csharp
//Using Configure function
using (var requester = new HttpRequester("http://localhost").Configure(cfg => cfg
    .WithBasicAuthorization("foo", "bar")
    .WithTimeout(TimeSpan.FromSeconds(30))
    .WithAccept("text/plain")))
{
    var response = await requester.GetAsync();
};
```

```csharp
//Using Config object
requester.Config
    .WithBasicAuthorization("foo", "bar")
    .WithTimeout(TimeSpan.FromSeconds(30))
    .WithAccept("text/plain");
```

### Requester.Validation NuGet
The [Requester.Validation package](https://www.nuget.org/packages/requester.validation), is used for validation of Web APIs.

```csharp
using Requester;
using Requester.Validation;
```

The samples below are written to work against a local CouchDB installation.

```csharp
When.Put("http://localhost:5984/mydb")
    .TheResponse(should => should.BeSuccessful());

When.Head("http://localhost:5984/mydb")
    .TheResponse(should => should.BeSuccessful());
```

#### Basic-Authentication
```csharp
When.Put("http://localhost:5984/mydb", cfg => cfg
    .WithBasicAuthorization("foo", "bar"))
    .TheResponse(should => should.BeSuccessful());
```

#### Custom headers

```csharp
When.Put(DbUrl, cfg => cfg
    .WithHeader("foo", "bar"))
    .WithHeader(h => h.Accept, "application/json"))
    .TheResponse(should => should.BeSuccessful());
```

#### Expectations
The idea is that you can use whatever testing framework you want. Below, I'm using xUnit. But it could just as well be NUnit.

```csharp
public class Candy
{
    private const string DbUrl = "http://sa:test@localhost:5984/mydb/";

    [Fact]
    public void Can_eat_candy_like_a_monster()
    {
        When.Put(DbUrl)
            .TheResponse(should => should.BeSuccessful());

        When.Put(DbUrl, cfg => cfg
            .WithHeader(h => h.Accept, "application/json"))
            .TheResponse(should => should.BeSuccessful());

        When.Head(DbUrl)
            .TheResponse(should => should.BeSuccessful());

        When.PostOfJson(DbUrl,
                        "{\"_id\":\"doc1\",
                          \"name\": \"Daniel Wertheim\",
                          \"address\":{\"street\":\"One way\", \"zip\":12345},
                          \"hobbies\":[\"programming\",\"running\"]}")
            .TheResponse(should => should
                .BeSuccessful()
                .HaveStatus(HttpStatusCode.Created));

        When.PutOfJson(DbUrl + "doc2",
                       "{\"name\": \"Daniel Wertheim\",
                         \"address\":{\"street\":\"Two way\", \"zip\":54321},
                         \"hobbies\":[\"programming\",\"running\"]}")
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

#### Violations
Violations will show in your test runner. The sample below violates `HaveJsonConformingToSchema`.

```
My_test_one failed

Requester.Validation.RequesterAssertionException : Expected object to be conforming to specified JSON schema.
Failed when inspecting 'address.street' due to 'Invalid type.
Expected Integer but got String. Line 1, position 112.'

RequestUri: http://localhost:5984/mydb/doc1
RequestMethod: GET
Status: OK(200)
Reason: OK
ETag: 1-23c4402e369a7a56059d912e53d320ee
ContentType:application/json
HasContent:True
Content:<NOT BEING SHOWED>
```

### Custom testing framework assertion exceptions
To keep down some dependencies, Requester throws a custom `RequesterAssertionException` but if you want your specific testing framework exceptions, just hook them in:

```csharp
AssertionExceptionFactory.ExceptionFn = msg => new NUnit.Framework.AssertionException(msg);
```
