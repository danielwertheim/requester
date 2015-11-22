using System;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Requester.FakeWebApi.Model;
using Requester.FakeWebApi.Storage;

namespace Requester.FakeWebApi.Controllers
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

        [HttpPost]
        [Route]
        public HttpResponseMessage Post(Person person)
        {
            person.Id = Guid.NewGuid();

            _personsStore.Store(person);

            return Request.CreateResponse(HttpStatusCode.Created, person);
        }

        [HttpPut]
        [Route]
        public HttpResponseMessage Put(Person person)
        {
            return Request.CreateResponse(_personsStore.Store(person) == StoreResult.Added ? HttpStatusCode.Created : HttpStatusCode.OK, person);
        }

        [HttpDelete]
        [Route("{id}")]
        public HttpResponseMessage Delete(Guid id)
        {
            return Request.CreateResponse(_personsStore.Delete(id) ? HttpStatusCode.OK : HttpStatusCode.NotFound);
        }
    }
}