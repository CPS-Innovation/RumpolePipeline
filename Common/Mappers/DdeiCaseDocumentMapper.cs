using Common.Domain.DocumentExtraction;
using Common.Domain.Extensions;
using Common.Domain.Responses;
using Common.Mappers.Contracts;

namespace Common.Mappers;

public class DdeiCaseDocumentMapper : ICaseDocumentMapper<DdeiCaseDocumentResponse>
{
    public CaseDocument Map(DdeiCaseDocumentResponse ddeiResponse)
    {
        if (ddeiResponse == null)
            return null;

        var result = new CaseDocument
        {
            DocumentId = ddeiResponse.Id.ToString(),
            VersionId = ddeiResponse.VersionId
        };

        if (string.IsNullOrWhiteSpace(ddeiResponse.OriginalFileName))
        {
            if (string.IsNullOrWhiteSpace(ddeiResponse.MimeType))
                return null;

            var fileExt = ddeiResponse.MimeType.GetExtension();
            if (string.IsNullOrWhiteSpace(fileExt))
                return null;

            result.FileName = string.Concat(ddeiResponse.Id.ToString(), ".", fileExt);
        }
        else
        {
            result.FileName = ddeiResponse.OriginalFileName;
        }

        result.CmsDocType = new CmsDocType
        {
            Code = ddeiResponse.DocumentType,
            Name = ddeiResponse.CmsDocCategory
        };

        return result;
    }
}
