﻿using AutoFixture;
using common.Wrappers;
using FluentAssertions;
using Xunit;

namespace rumpolepipeline.tests.common.Wrappers
{
	public class ValidatorWrapperTests
	{
        private readonly Fixture _fixture;

        public ValidatorWrapperTests()
        {
            _fixture = new Fixture();
        }

        [Fact]
        public void Validate_ReturnsEmptyValidationResultsWhenValidRequest()
        {
            var request = _fixture.Create<StubRequest>();

            var results = new ValidatorWrapper<StubRequest>().Validate(request);

            results.Should().BeEmpty();
        }

        [Fact]
        public void Validate_ReturnsNonEmptyValidationResultsWhenInvalidRequest()
        {
            var request = new StubRequest();

            var results = new ValidatorWrapper<StubRequest>().Validate(request);

            results.Should().NotBeEmpty();
        }
    }
}
