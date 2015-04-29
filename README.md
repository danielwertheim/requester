# Requester
Requester is a Http-request fiddling magical something that is designed to help me, and maybe you as well, to validate APIs. It's mainly focused on APIs that work with JSON. It was put together after some fiddling with an awesome NodeJS peer: [FrisbyJS](http://frisbyjs.com/ "FrisbyJS").

## Disclaimer, Remarks...
This is still an early release and the API and license might change. And even do you could use `Requester` to interact with APIs without the intentions of validating it, that's not really the purpose. The purpose is the validate responses from APIs.

## Samples
The samples below are written to work against a local CouchDB installation. By default, it's initialized to `Accept: application/json` hence:

```csharp
DoRequest
	.Against("http://localhost:5984/mydb")
	.UsingPostAsync(@"{""name"":""Daniel Wertheim""}")
```

is the same as:

```csharp
DoRequest
	.Against("http://localhost:5984/mydb")
	.WithAccept(i => i.ApplicationJson)
	.UsingPostAsync(@"{""name"":""Daniel Wertheim""}")
```

### Base URL vs Relative URL
The URL you specify to: `DoRequest.Against("...")` is the base URL. You can also provide an optional, relative URL, for each operation: `HEAD, GET, POST, PUT, DELETE`, using the overloads that accept it:

```csharp
DoRequest
	.Against("http://localhost:5984/mydb/danielwertheim")
	.UsingPutAsync(@"{""name"":""Daniel Wertheim""}");

//vs

DoRequest
	.Against("http://localhost:5984/mydb")
	.UsingPutAsync(@"{""name"":""Daniel Wertheim""}", "danielwertheim");
```

### Authentication
For now only basic authentication is supported and it can be defined in two ways: via URL or using `WithAuthentication`.

```csharp
DoRequest
	.Against("http://myuser:mypwd@localhost:5984/mydb")

//vs

DoRequest
	.Against("http://localhost:5984/mydb")
	.WithBasicAuthorization("myuser", "mypwd")
```

**Please note**, that when passing in URL, you need to encode your values.

### Custom headers
```csharp
DoRequest
	.Against("http://localhost:5984/mydb")
	.WithHeader("X-Foo", "bar")
```

### Validation & Expectations - HttpResponse.IsExpectedTo()
The idea is that you use your prefered testing framework and the use `Requester` to help you validate your APIs. The methods: `UsingHeadAsync, UsingGetAsync, UsingPostAsync, UsingPutAsync, UsingDeleteAsync` all returns a `HttpResponse` which has a bunch of validation helpers.

First, include the `namespace Requester.Validation`. Then access all validation/expectations helper using: `response.IsExpectedTo()`

```csharp
[xUnit.Fact]
public void Should_be_able_to_get_json_document()
{
	(await DoRequest
		.Against("http://localhost:5984/mydb")
		.UsingGetAsync("danielwertheim"))
		.IsExpectedTo()

	    .BeSuccessful()
	    .HaveStatus(HttpStatusCode.OK)
	    .BeJsonResponse()
	    .HaveAnyContent()
	    .HaveJsonConformingToSchema(@"{
	        _id: {type: 'string', required: true},
	        _rev: {type: 'string', required: true},
	        name: {type: 'string'},
	        address: {type: 'object', properties: {zip: {type: 'integer'}}},
	        hobbies: {type: 'array', items: {type: 'string'}}
	    }")
	    .Match(new {_id = "danielwertheim", name = "Daniel Wertheim", hobbies = new [] {"Programming", "Running"}})
	    .HaveSpecificValue("_id", "danielwertheim")
	    .HaveSpecificValue("hobbies[0]", "Programming")
	    .HaveSpecificValue("address.zip", 12345);
}
```

You can of course use `ContinueWith`:

```csharp
await DoRequest
	.Against("http://localhost:5984/mydb")
	.UsingGetAsync()
	.ContinueWith(t => t.Result.IsExpectedTo().BeSuccessful());
```

### Custom testing framework assertion exceptions
To keep down some dependencies, Requester throws a custom `RequesterAssertionException` but if you want your specific testing framework exceptions, just hook them in:

```csharp
AssertionExceptionFactory.ExceptionFn = msg => new NUnit.Framework.AssertionException(msg);
```

### Override some static default configurations
There are some static extension points you can make use of to customize behavior and to hook in custom implementations.

```csharp
//Default encoding
DoRequest.DefaultEncoding = Encoding.UTF8;

//Custom initializer
DoRequest.Initializer = request => request.WithAccept(i => i.ApplicationJson);

//Hook in your custom IDoRequest implementation
DoRequest.Factory = uri => new MyIDoRequestImplementation(uri);
```

### Reuse the IDoRequest
The methods: `HeadAsync, GetAsync, PostAsync, PutAsync, DeleteAsync` all returns a `HttpResponse`. All other methods returns an `IDoRequest` instance. You can therefore reuse it if you want.

```csharp
var iDoRequest = DoRequest.Against("http://localhost:5984/mydb");

var dbCreatedResponse = await iDoRequest.UsingPutAsync();

var docCreatedResponse = await iDoRequest.UsingPutAsync(@"{""name"":""Daniel Wertheim""}", "danielwertheim");

var deleteDocResponse = await iDoRequest.UsingDeleteAsync("danielwertheim?rev=" + docCreatedResponse.ETag);
```
