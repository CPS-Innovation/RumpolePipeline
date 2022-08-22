using AutoFixture;
using Common.Domain.Extensions;
using Common.Domain.Validation;
using Common.tests.Wrappers;
using FluentAssertions;
using FluentAssertions.Execution;
using FluentValidation.Results;
using Xunit;

namespace Common.tests.Extensions
{
    public class ValidatableRequestTests
    {
        private readonly Fixture _fixture;

        public ValidatableRequestTests()
        {
            _fixture = new Fixture();
        }

        [Fact]
        public void ValidatableRequest_OnCallingFlattenErrors_WithoutErrors_ReturnsEmptyString()
        {
            var request = _fixture.Create<ValidatableRequest<StubRequest>>();
            request.IsValid = true;
            request.Errors.Clear();

            var result = request.FlattenErrors();

            using (new AssertionScope())
            {
                request.Value.Should().BeAssignableTo<StubRequest>();
                request.IsValid.Should().BeTrue();
                result.Should().BeEmpty();
            }
        }

        [Fact]
        public void ValidatableRequest_OnCallingFlattenErrors_WithKnownErrors_ReturnsCorrectConcatenatedString()
        {
            var request = _fixture.Create<ValidatableRequest<StubRequest>>();
            request.IsValid = false;
            request.Errors = new List<ValidationFailure>
            {
                new("Field1", "Error Message 1"),
                new("Field2", "Error Message 2")
            };

            var result = request.FlattenErrors();

            using (new AssertionScope())
            {
                request.Value.Should().BeAssignableTo<StubRequest>();
                request.IsValid.Should().BeFalse();
                result.Should().Be("{ Field = Field1, Error = Error Message 1 }\r\n{ Field = Field2, Error = Error Message 2 }");
            }
        }
    }
}
