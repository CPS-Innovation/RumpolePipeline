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

        var result = new CaseDocument(ddeiResponse.Id.ToString(), ddeiResponse.VersionId, ddeiResponse.DocumentType, ddeiResponse.CmsDocCategory);

        if (string.IsNullOrWhiteSpace(ddeiResponse.OriginalFileName))
        {
            if (string.IsNullOrWhiteSpace(ddeiResponse.MimeType))
                return null;

            var fileExt = ddeiResponse.MimeType.GetExtension();
            if (string.IsNullOrWhiteSpace(fileExt))
                return null;

            result.FileName = string.Concat(ddeiResponse.Id.ToString(), fileExt);
        }
        else
        {
            result.FileName = ddeiResponse.OriginalFileName;
        }

        return result;
    }
}
