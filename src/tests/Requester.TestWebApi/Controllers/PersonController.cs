using System;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Requester.TestWebApi.Model;
using Requester.TestWebApi.Storage;

namespace Requester.TestWebApi.Controllers
{
    [RoutePrefix("api/persons")]
    public class PersonController : ApiController
    {
        private readonly IPersonsStore _personsStore;

        public PersonController(IPersonsStore personsStore)
        {
            _personsStore = personsStore;
        }

        [HttpGet]
        [Route("{id}")]
        public HttpResponseMessage Get(Guid id)
        {
            var person = _personsStore.Get(id);
            if (person != null)
                return Request.CreateResponse(HttpStatusCode.OK, person);

            return Request.CreateResponse(HttpStatusCode.NotFound);
        }

        [HttpPut]
        [Route]
        public HttpResponseMessage Put(Person person)
        {
            _personsStore.Store(person);

            return Request.CreateResponse(HttpStatusCode.Created);
        }
    }
}