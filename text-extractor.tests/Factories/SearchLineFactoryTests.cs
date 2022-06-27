using AutoFixture;
using FluentAssertions;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;
using text_extractor.Domain;
using text_extractor.Factories;
using Xunit;

namespace text_extractor.tests.Factories
{
	public class SearchLineFactoryTests
	{
		private Fixture _fixture;
		private int _caseId;
		private string _documentId;
		private ReadResult _readResult;
		private Line _line;
		private int _index;

		private ISearchLineFactory SearchLineFactory;

		public SearchLineFactoryTests()
        {
			_fixture = new Fixture();
			_caseId = _fixture.Create<int>();
			_documentId = _fixture.Create<string>();
			_readResult = new ReadResult
			{
				Page = _fixture.Create<int>()
			};
			_line = _fixture.Create<Line>();
			_index = _fixture.Create<int>();

			SearchLineFactory = new SearchLineFactory();
		}

		[Fact]
		public void Create_ReturnsExpectedId()
		{
			var factory = SearchLineFactory.Create(_caseId, _documentId, _readResult, _line, _index);

			factory.Id.Should().Be($"{_caseId}-{_documentId}-{_readResult.Page}-{_index}");
		}

		[Fact]
		public void Create_ReturnsExpectedCaseId()
		{
			var factory = SearchLineFactory.Create(_caseId, _documentId, _readResult, _line, _index);

			factory.CaseId.Should().Be(_caseId);
		}

		[Fact]
		public void Create_ReturnsExpectedDocumentId()
		{
			var factory = SearchLineFactory.Create(_caseId, _documentId, _readResult, _line, _index);

			factory.DocumentId.Should().Be(_documentId);
		}

		[Fact]
		public void Create_ReturnsExpectedPageIndex()
		{
			var factory = SearchLineFactory.Create(_caseId, _documentId, _readResult, _line, _index);

			factory.PageIndex.Should().Be(_readResult.Page);
		}

		[Fact]
		public void Create_ReturnsExpectedLineIndex()
		{
			var factory = SearchLineFactory.Create(_caseId, _documentId, _readResult, _line, _index);

			factory.LineIndex.Should().Be(_index);
		}

		[Fact]
		public void Create_ReturnsExpectedLanguage()
		{
			var factory = SearchLineFactory.Create(_caseId, _documentId, _readResult, _line, _index);

			factory.Language.Should().Be(_line.Language);
		}

		[Fact]
		public void Create_ReturnsExpectedBoundingBox()
		{
			var factory = SearchLineFactory.Create(_caseId, _documentId, _readResult, _line, _index);

			factory.BoundingBox.Should().BeEquivalentTo(_line.BoundingBox);
		}

		[Fact]
		public void Create_ReturnsExpectedAppearance()
		{
			var factory = SearchLineFactory.Create(_caseId, _documentId, _readResult, _line, _index);

			factory.Appearance.Should().Be(_line.Appearance);
		}

		[Fact]
		public void Create_ReturnsExpectedText()
		{
			var factory = SearchLineFactory.Create(_caseId, _documentId, _readResult, _line, _index);

			factory.Text.Should().Be(_line.Text);
		}

		[Fact]
		public void Create_ReturnsExpectedWords()
		{
			var factory = SearchLineFactory.Create(_caseId, _documentId, _readResult, _line, _index);

			factory.Words.Should().BeEquivalentTo(_line.Words);
		}
	}
}

