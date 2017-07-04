using System;
using FakeWebApi.Model;

namespace FakeWebApi.Storage
{
    public interface IPersonsStore
    {
        StoreResult Store(Person person);
        Person Get(Guid id);
        bool Delete(Guid id);
    }
}