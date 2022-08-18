using AutoFixture;
using common.Wrappers;
using Common.tests.Wrappers;
using FluentAssertions;
using Newtonsoft.Json;
using Xunit;

namespace common.tests.Wrappers
{
    public class JsonConvertWrapperTests
    {
        private Fixture _fixture;

        public JsonConvertWrapperTests()
        {
            _fixture = new Fixture();
        }

        [Fact]
        public void SerializeObjectShouldDelegate()
        {
            var testObject = _fixture.Create<StubRequest>();
            var expectedSerialisedRequest = JsonConvert.SerializeObject(testObject, Formatting.None, new JsonSerializerSettings());

            var serialisedObject = new JsonConvertWrapper().SerializeObject(testObject);

            serialisedObject.Should().BeEquivalentTo(expectedSerialisedRequest);
        }

        [Fact]
        public void DeserializeObjectShouldDelegate()
        {
            var testObject = _fixture.Create<StubRequest>();
            var serialisedRequest = JsonConvert.SerializeObject(testObject, Formatting.None, new JsonSerializerSettings());

            var deserialisedRequest = new JsonConvertWrapper().DeserializeObject<StubRequest>(serialisedRequest);

            testObject.Should().BeEquivalentTo(deserialisedRequest);
        }
    }
}
