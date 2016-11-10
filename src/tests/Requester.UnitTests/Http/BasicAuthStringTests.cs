using FluentAssertions;
using Requester.Http;
using Xunit;

namespace Requester.UnitTests.Http
{
    public class BasicAuthStringTests : UnitTestsOf<BasicAuthorizationString>
    {
        [Fact]
        public void When_passing_username_and_password_It_initializes_a_base64_encoded_string()
        {
            UnitUnderTest = new BasicAuthorizationString("testUser", "testPassword");

            UnitUnderTest.Value.Should().Be("dGVzdFVzZXI6dGVzdFBhc3N3b3Jk");
            UnitUnderTest.ToString().Should().Be(UnitUnderTest.Value);
            UnitUnderTest.Should().Be(UnitUnderTest.Value);
        }

        [Fact]
        public void When_comparing_against_equal_string_It_should_return_true()
        {
            UnitUnderTest = new BasicAuthorizationString("testUser", "testPassword");

            UnitUnderTest.Equals("dGVzdFVzZXI6dGVzdFBhc3N3b3Jk").Should().BeTrue();
        }

        [Fact]
        public void When_comparing_against_non_equal_string_It_should_return_false()
        {
            UnitUnderTest = new BasicAuthorizationString("testUser", "testPassword");

            UnitUnderTest.Equals("tester_joe").Should().BeFalse();
        }

        [Fact]
        public void When_comparing_against_equal_object_It_should_return_true()
        {
            UnitUnderTest = new BasicAuthorizationString("testUser", "testPassword");

            UnitUnderTest.Equals(new BasicAuthorizationString("testUser", "testPassword")).Should().BeTrue();
        }

        [Fact]
        public void When_comparing_against_non_equal_object_It_should_return_false()
        {
            UnitUnderTest = new BasicAuthorizationString("testUser", "testPassword");

            UnitUnderTest.Equals(new BasicAuthorizationString("testerjoe", "faker")).Should().BeFalse();
        }
    }
}