[![Nuget](https://img.shields.io/nuget/v/requester.svg)](https://www.nuget.org/packages/requester/) [![Users](https://img.shields.io/nuget/dt/requester.svg)](https://www.nuget.org/packages/requester/)

# Requester
Requester is a Http-request fiddling magical something that is designed to help me, and maybe you as well, to validate APIs. It's mainly focused on APIs that work with JSON. It was put together after some fiddling with an awesome NodeJS peer: [FrisbyJS](http://frisbyjs.com/ "FrisbyJS").

## About the version
Until `v1.0.0` the API should be seen as non stable and might target to change.

## Disclaimer, Remarks...
This is still an early release hence stuff WILL change. Especially additions for URL-building and helpers for doing requests that should not be validated.

## NuGet
[Of course](https://www.nuget.org/packages/requester). Just do:

	install-package requester

Then import the namespaces:

	using Requester;
	using Requester.Validation;

## Samples
The samples below are written to work against a local CouchDB installation.

	When.Put("http://localhost:5984/mydb")
	    .TheResponse(should => should.BeSuccessful());
	
	When.Head("http://localhost:5984/mydb")
	    .TheResponse(should => should.BeSuccessful());

### Authentication
For now, only basic authentication is supported and it can be defined in two ways: via URL or using `WithAuthentication`.

	When.Put("http://foo:bar@localhost:5984/mydb")
		.TheResponse(should => should.BeSuccessful());
	
	//vs
	
	When.Put("http://localhost:5984/mydb", cfg => cfg
	    .WithBasicAuthorization("foo", "bar"))
	    .TheResponse(should => should.BeSuccessful());

**Please note**, that when passing credentials in the URL, you need to encode your values.

### Custom headers

	When.Put(DbUrl, cfg => cfg
	    .WithHeader("foo", "bar"))
		.WithHeader(h => h.Accept, "application/json"))
	    .TheResponse(should => should.BeSuccessful());

### Expectations
The idea is that you can use whatever testing framework you want. Below, I'm using xUnit. But it could just as well be NUnit.

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


Violations will show in your test runner. The sample below violates `HaveJsonConformingToSchema`.

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

### HttpRequester
The `HttpRequester` is what is used but the `When` constructs, and can of course be used to perform Http-requests.

	using(var requester = new HttpRequester("http://localhost:5984/mydb"))
	{
	  //Ensure db is created
	  await requester.SendAsync(new HttpRequest(HttpMethod.Put));
	
	  //Create a document
	  await requester.SendAsync(
	    new HttpRequest(HttpMethod.Put, "/mydocid").WithJsonContent(someJson));
		
	  //Our using simplified overloads...
	  await requester.PostAsync();
	  await requester.PostJsonAsync(json);
	  await requester.PostEntityAsync(new { _id = "doc2" Name = "Foo" });
	  
	  await requester.PutAsync();
	  await requester.PutJsonAsync(json, "/doc3");
	  await requester.PutEntityAsync(new { Name = "Foo" }, "/doc4");
	}

### HttpResponse
The response of the `HttpRequester` is a `HttpResponse`.


	//Everything is a response (HEAD, GET, POST, PUT, DELETE)
	var get = await requester.SendAsync(new HttpRequest(HttpMethod.Get, "/mydocid"));
	
	//The resonse has like: StatusCode, Reason, Content, ETag, ContentType etc.
	Debug.WriteLine(get.ToStringDebugVersion(includeContent: true));

The out put would be:

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
 
### Custom testing framework assertion exceptions
To keep down some dependencies, Requester throws a custom `RequesterAssertionException` but if you want your specific testing framework exceptions, just hook them in:

	AssertionExceptionFactory.ExceptionFn = msg => new NUnit.Framework.AssertionException(msg);



