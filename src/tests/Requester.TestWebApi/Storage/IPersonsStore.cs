using System;
using Requester.TestWebApi.Model;

namespace Requester.TestWebApi.Storage
{
    public interface IPersonsStore
    {
        void Store(Person person);
        Person Get(Guid id);
    }
}