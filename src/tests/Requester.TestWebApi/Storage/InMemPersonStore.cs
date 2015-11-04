using System;
using System.Collections.Concurrent;
using Requester.TestWebApi.Model;

namespace Requester.TestWebApi.Storage
{
    public class InMemPersonStore : IPersonsStore
    {
        private readonly ConcurrentDictionary<Guid, Person> _personState = new ConcurrentDictionary<Guid, Person>();

        public StoreResult Store(Person person)
        {
            var m = StoreResult.Added;

            _personState.AddOrUpdate(person.Id, person, (eid, ep) =>
            {
                m = StoreResult.Updated;
                return person;
            });

            return m;
        }

        public Person Get(Guid id)
        {
            Person r;
            return _personState.TryGetValue(id, out r) ? r : null;
        }

        public bool Delete(Guid id)
        {
            Person r;
            return _personState.TryRemove(id, out r);
        }
    }
}