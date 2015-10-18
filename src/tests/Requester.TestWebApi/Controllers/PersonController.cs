using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Requester.TestWebApi.Model;

namespace Requester.TestWebApi.Controllers
{
    [RoutePrefix("api/persons")]
    public class PersonController : ApiController
    {
        private static readonly ConcurrentDictionary<Guid, Person> PersonState = new ConcurrentDictionary<Guid, Person>();

        [HttpGet]
        [Route("{id}")]
        public HttpResponseMessage Get(Guid id)
        {
            Person r;
            if (PersonState.TryGetValue(id, out r))
                return Request.CreateResponse(HttpStatusCode.OK, r);

            return Request.CreateResponse(HttpStatusCode.NotFound);
        }

        [HttpPut]
        [Route]
        public void Put(Person person)
        {
            PersonState.AddOrUpdate(person.Id, person, (eid, ep) => person);
        }
    }
}