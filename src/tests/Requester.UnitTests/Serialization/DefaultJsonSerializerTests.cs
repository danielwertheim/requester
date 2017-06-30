using System.Collections.Generic;
using FluentAssertions;
using Requester.Serialization;
using Xunit;

namespace Requester.UnitTests.Serialization
{
    public class DefaultJsonSerializerTests : UnitTestsOf<DefaultJsonSerializer>
    {
        public DefaultJsonSerializerTests()
        {
            UnitUnderTest = new DefaultJsonSerializer();
        }

        [Fact]
        public void Should_not_touch_dictionary_keys()
        {
            var orgData = new SomeData
            {
                SomeKeys = new Dictionary<string, int>
                {
                    { "TestString1", 1 },
                    { "TESTSTRING2", 2 },
                    { "teststring3", 3 },
                }
            };

            var json = UnitUnderTest.Serialize(orgData);

            var reconstructed = UnitUnderTest.Deserialize<SomeData>(json);

            foreach (var kv in orgData.SomeKeys)
                reconstructed.SomeKeys.ContainsKey(kv.Key).Should().BeTrue();
        }

        private class SomeData
        {
            public Dictionary<string, int> SomeKeys { get; set; }
        }
    }
}