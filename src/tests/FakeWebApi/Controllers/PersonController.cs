using System;
using System.Net;
using FakeWebApi.Model;
using FakeWebApi.Storage;
using Microsoft.AspNetCore.Mvc;

namespace FakeWebApi.Controllers
{
    [Route("api/persons")]
    public class PersonController : Controller
    {
        private readonly IPersonsStore _personsStore;

        public PersonController(IPersonsStore personsStore)
        {
            _personsStore = personsStore;
        }

        [HttpGet("{id}")]
        public IActionResult Get(Guid id)
        {
            var person = _personsStore.Get(id);
            if (person != null)
                return Ok(person);

            return NotFound();
        }

        [HttpPost]
        public IActionResult Post([FromBody]Person person)
        {
            person.Id = Guid.NewGuid();

            _personsStore.Store(person);

            return StatusCode((int)HttpStatusCode.Created, person);
        }

        [HttpPut]
        public IActionResult Put([FromBody]Person person)
        {
            return StatusCode((int)(_personsStore.Store(person) == StoreResult.Added
                ? HttpStatusCode.Created
                : HttpStatusCode.OK),
                person);
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(Guid id)
        {
            return StatusCode((int)(_personsStore.Delete(id) ? HttpStatusCode.OK : HttpStatusCode.NotFound));
        }
    }
}