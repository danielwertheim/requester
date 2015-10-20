using System;
using System.Collections.Concurrent;
using Requester.TestWebApi.Model;

namespace Requester.TestWebApi.Storage
{
    public class InMemPersonStore : IPersonsStore
    {
        private readonly ConcurrentDictionary<Guid, Person> _personState = new ConcurrentDictionary<Guid, Person>();

        public void Store(Person person)
        {
            _personState.AddOrUpdate(person.Id, person, (eid, ep) => person);
        }

        public Person Get(Guid id)
        {
            Person r;
            return _personState.TryGetValue(id, out r) ? r : null;
        }
    }
}