using System;
using Requester.FakeWebApi.Model;

namespace Requester.FakeWebApi.Storage
{
    public interface IPersonsStore
    {
        StoreResult Store(Person person);
        Person Get(Guid id);
        bool Delete(Guid id);
    }
}