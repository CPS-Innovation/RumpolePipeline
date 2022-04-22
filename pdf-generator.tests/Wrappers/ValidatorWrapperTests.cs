using AutoFixture;
using FluentAssertions;
using pdf_generator.Domain.Requests;
using pdf_generator.Wrappers;
using Xunit;

namespace pdf_generator.tests.Wrappers
{
	public class ValidatorWrapperTests
	{
        private Fixture _fixture;

        public ValidatorWrapperTests()
        {
            _fixture = new Fixture();
        }

        [Theory]
        [InlineData("Test.doc")]
        [InlineData("Test2.xlsx")]
        public void Validate_GeneratePdfRequest_ReturnsNonEmptyValidationResultsWhenFileNameIsValid(string fileName)
        {
            var request = _fixture.Build<GeneratePdfRequest>()
                            .With(r => r.FileName, fileName)
                            .Create();

            var results = new ValidatorWrapper<GeneratePdfRequest>().Validate(request);

            results.Should().BeEmpty();
        }

        [Fact]
        public void Validate_GeneratePdfRequest_ReturnsNonEmptyValidationResultsWhenCaseIdIsMissing()
        {
            var request = _fixture.Build<GeneratePdfRequest>()
                            .With(r => r.CaseId, null)
                            .With(r => r.FileName, "TestFile.doc")
                            .Create();

            var results = new ValidatorWrapper<GeneratePdfRequest>().Validate(request);

            results.Should().NotBeEmpty();
        }

        [Fact]
        public void Validate_GeneratePdfRequest_ReturnsNonEmptyValidationResultsWhenDocumentIdIsMissing()
        {
            var request = _fixture.Build<GeneratePdfRequest>()
                            .With(r => r.DocumentId, default(string))
                            .With(r => r.FileName, "TestFile.doc")
                            .Create();

            var results = new ValidatorWrapper<GeneratePdfRequest>().Validate(request);

            results.Should().NotBeEmpty();
        }

        [Fact]
        public void Validate_GeneratePdfRequest_ReturnsNonEmptyValidationResultsWhenFileNameIsMissing()
        {
            var request = _fixture.Build<GeneratePdfRequest>()
                            .With(r => r.FileName, default(string))
                            .Create();

            var results = new ValidatorWrapper<GeneratePdfRequest>().Validate(request);

            results.Should().NotBeEmpty();
        }

        [Fact]
        public void Validate_GeneratePdfRequest_ReturnsNonEmptyValidationResultsWhenFileNameIsInvalid()
        {
            var request = _fixture.Build<GeneratePdfRequest>()
                            .With(r => r.FileName, "12345")
                            .Create();

            var results = new ValidatorWrapper<GeneratePdfRequest>().Validate(request);

            results.Should().NotBeEmpty();
        }
    }
}

