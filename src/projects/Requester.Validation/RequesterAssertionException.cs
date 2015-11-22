using System;

namespace Requester.Validation
{
    public class RequesterAssertionException : Exception
    {
        public RequesterAssertionException(string message) : base(message) { }
    }
}