using AutoFixture;
using System.Text;
using Common.Domain.Extensions;
using Common.tests.Wrappers;
using FluentAssertions;
using FluentAssertions.Execution;
using Xunit;
using Newtonsoft.Json;

namespace Common.tests.Extensions
{
    public class HttpRequestMessageTests
    {
        private readonly Fixture _fixture = new();

        [Fact]
        public async Task WhenHttpRequestHasNoContent_ThrowsArgumentNullException()
        {
            var request = new HttpRequestMessage();
            request.Content = null;
            
            var act = async () => await request.GetJsonBody<StubRequest>();

            await act.Should().ThrowAsync<ArgumentNullException>();
        }

        [Fact]
        public async Task WhenHttpRequestHasContent_ShouldReturnCorrectType()
        {
            var request = new HttpRequestMessage();
            var testContent = _fixture.Create<StubRequest>();
            var requestJson = JsonConvert.SerializeObject(testContent);
            request.Content = new StringContent(requestJson, Encoding.UTF8, "application/json");

            var result = await request.GetJsonBody<StubRequest>();

            using (new AssertionScope())
            {
                result.Should().NotBeNull();
                result.StubString.Should().Be(testContent.StubString);
            }
        }

        [Fact]
        public async Task WhenHttpRequestHasValidatableContent_IfInvalid_ShouldReturnThatObjectWith_ValidationDetails()
        {
            var request = new HttpRequestMessage();
            var testContent = _fixture.Create<StubRequest>();
            testContent.StubString = string.Empty;
            var requestJson = JsonConvert.SerializeObject(testContent);
            request.Content = new StringContent(requestJson, Encoding.UTF8, "application/json");

            var result = await request.GetJsonBody<ValidatableStubRequest, ValidatableStubRequestValidator>();

            using (new AssertionScope())
            {
                result.Should().NotBeNull();
                result.IsValid.Should().BeFalse();
                result.Errors.Count.Should().Be(1);
                result.Errors[0].ErrorMessage.Should().NotBeEmpty();
            }
        }

        [Fact]
        public async Task WhenHttpRequestHasValidatableContent_IfValid_ShouldReturnThatObjectWithAPositive_IsValidStatus()
        {
            var request = new HttpRequestMessage();
            var testContent = _fixture.Create<StubRequest>();
            var requestJson = JsonConvert.SerializeObject(testContent);
            request.Content = new StringContent(requestJson, Encoding.UTF8, "application/json");

            var result = await request.GetJsonBody<ValidatableStubRequest, ValidatableStubRequestValidator>();

            using (new AssertionScope())
            {
                result.Should().NotBeNull();
                result.IsValid.Should().BeTrue();
                result.Errors.Should().BeNull();
                result.Value.Should().BeAssignableTo<ValidatableStubRequest>();
            }
        }
    }
}
