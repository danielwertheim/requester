using Xunit;

namespace Requester.UnitTests
{
    [Trait("Category", "UnitTests")]
    public abstract class UnitTestsOf<T> : UnitTests where T : class
    {
        protected T UnitUnderTest { get; set; }
    }

    [Trait("Category", "UnitTests")]
    public abstract class UnitTests { }
}