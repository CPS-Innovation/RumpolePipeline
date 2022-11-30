using AutoFixture;
using Common.Constants;
using Common.Domain.Responses;
using Common.Mappers;
using Common.Mappers.Contracts;
using FluentAssertions;
using Xunit;

namespace Common.tests.Mappers;

public class DdeiCaseDocumentMapperTests
{
    private readonly ICaseDocumentMapper<DdeiCaseDocumentResponse> _mapper;
    private readonly DdeiCaseDocumentResponse _documentResponse;

    public DdeiCaseDocumentMapperTests()
    {
        var fixture = new Fixture();
        _mapper = new DdeiCaseDocumentMapper();
        _documentResponse = fixture.Create<DdeiCaseDocumentResponse>();
    }

    [Fact]
    public void When_AllElementsArePresentInResponse_ReturnsCorrectValues()
    {
        var result = _mapper.Map(_documentResponse);

        result.DocumentId.Should().Be(_documentResponse.Id.ToString());
        result.FileName.Should().Be(_documentResponse.OriginalFileName);
        result.VersionId.Should().Be(_documentResponse.VersionId);
        result.CmsDocType.Name.Should().Be(_documentResponse.CmsDocCategory);
        result.CmsDocType.Code.Should().Be(_documentResponse.TypeId);
    }
    
    [Fact]
    public void When_OriginalFileNameIsNullInResponse_ReturnsCorrectValues()
    {
        _documentResponse.OriginalFileName = null;
        
        var result = _mapper.Map(_documentResponse);

        result.DocumentId.Should().Be(_documentResponse.Id.ToString());
        result.FileName.Should().Be(_documentResponse.OriginalFileName);
        result.VersionId.Should().Be(_documentResponse.VersionId);
        result.CmsDocType.Name.Should().Be(_documentResponse.CmsDocCategory);
        result.CmsDocType.Code.Should().Be(_documentResponse.TypeId);
    }

    [Fact] 
    public void When_DocumentTypeIsNullInResponse_ReturnsCorrectValues_AndUnknownAsDocumentType()
    {
        _documentResponse.TypeId = null;
        
        var result = _mapper.Map(_documentResponse);

        result.DocumentId.Should().Be(_documentResponse.Id.ToString());
        result.FileName.Should().Be(_documentResponse.OriginalFileName);
        result.VersionId.Should().Be(_documentResponse.VersionId);
        result.CmsDocType.Name.Should().Be(_documentResponse.CmsDocCategory);
        result.CmsDocType.Code.Should().Be(MiscCategories.UnknownDocumentType);
    }
}
