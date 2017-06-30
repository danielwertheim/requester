using System;
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
            var orgData = new HasDictionary
            {
                SomeKeys = new Dictionary<string, int>
                {
                    { "TestString1", 1 },
                    { "TESTSTRING2", 2 },
                    { "teststring3", 3 },
                }
            };

            var json = UnitUnderTest.Serialize(orgData);

            var reconstructed = UnitUnderTest.Deserialize<HasDictionary>(json);

            foreach (var kv in orgData.SomeKeys)
                reconstructed.SomeKeys.ContainsKey(kv.Key).Should().BeTrue();
        }

        [Fact]
        public void Should_not_die_When_json_contains_more_members()
        {
            var orgData = new
            {
                MyString = "Test",
                MyInt = 42
            };

            var json = UnitUnderTest.Serialize(orgData);

            Action a = () => UnitUnderTest.Deserialize<HasOnlyString>(json);
            
            a.ShouldNotThrow();
        }

        private class HasDictionary
        {
            public Dictionary<string, int> SomeKeys { get; set; }
        }

        private class HasOnlyString
        {
            public string StringValue { get; set; }
        }
    }
}