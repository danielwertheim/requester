using System;
using Requester.TestWebApi.Model;

namespace Requester.TestWebApi.Storage
{
    public interface IPersonsStore
    {
        StoreResult Store(Person person);
        Person Get(Guid id);
        bool Delete(Guid id);
    }
}