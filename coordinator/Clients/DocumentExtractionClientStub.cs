using System;
using System.Globalization;
using System.Threading.Tasks;
using Common.Constants;
using Common.Domain.DocumentExtraction;
using Common.Domain.Extensions;
using Common.Logging;
using coordinator.TempService;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace coordinator.Clients
{
	public class DocumentExtractionClientStub : IDocumentExtractionClient
    {
        private readonly ILogger<DocumentExtractionClientStub> _logger;
        private readonly IConfiguration _configuration;
        private readonly IBlobStorageService _blobStorageService;

        public DocumentExtractionClientStub(ILogger<DocumentExtractionClientStub> logger, IConfiguration configuration, IBlobStorageService blobStorageService)
        {
            _logger = logger;
            _configuration = configuration;
            _blobStorageService = blobStorageService;
        }

        public async Task<Case> GetCaseDocumentsAsync(string caseId, string accessToken, Guid correlationId)
        {
            _logger.LogMethodEntry(correlationId, nameof(GetCaseDocumentsAsync), caseId);

            Case result = null;
            var useEndToEnd = bool.Parse(_configuration[FeatureFlags.EvaluateDocuments]);

            if (useEndToEnd)
            {
                if (caseId is "18846" or "18848")
                    result = await _blobStorageService.GetDocumentsAsync(caseId, correlationId);
            }
            else
            {
                result = Task.FromResult(caseId switch
                {
                    "18846" => McLoveCase(caseId),
                    "18848" => MultipleFileTypeCase(caseId),
                    _ => null
                }).Result;
            }

            _logger.LogMethodExit(correlationId, nameof(GetCaseDocumentsAsync), result.ToJson());
            return result;
        }

        private static Case McLoveCase(string caseId)
        {
            var dt = DateTime.Now;
            var iv = CultureInfo.InvariantCulture;
            return new Case
            {
                CaseId = caseId,
                CaseDocuments = new[]
                {
                    new CaseDocument
                    {
                        DocumentId = "MG12",
                        LastUpdatedDate = dt.ToString("s", iv),
                        FileName = "MG12.doc",
                        CmsDocType = new CmsDocType
                        {
                            Code = "MG12",
                            Name = "MG12 File"
                        }
                    },
                    new CaseDocument
                    {
                        DocumentId = "stmt Shelagh McLove MG11",
                        LastUpdatedDate = dt.ToString("s", iv),
                        FileName = "stmt Shelagh McLove MG11.docx",
                        CmsDocType = new CmsDocType
                        {
                            Code = "MG11",
                            Name = "MG11 File"
                        }
                    },
                    new CaseDocument
                    {
                        DocumentId = "MG00",
                        LastUpdatedDate = dt.ToString("s", iv),
                        FileName = "MG00.doc",
                        CmsDocType = new CmsDocType
                        {
                            Code = "MG00",
                            Name = "MG00 File"
                        }
                    },
                    new CaseDocument
                    {
                        DocumentId = "stmt JONES 1989 1 JUNE mg11",
                        LastUpdatedDate = dt.ToString("s", iv),
                        FileName = "stmt JONES 1989 1 JUNE mg11.docx",
                        CmsDocType = new CmsDocType
                        {
                            Code = "MG11",
                            Name = "MG11 File"
                        }
                    },
                    new CaseDocument
                    {
                        DocumentId = "MG20 10 JUNE",
                        LastUpdatedDate = dt.ToString("s", iv),
                        FileName = "MG20 10 JUNE.doc",
                        CmsDocType = new CmsDocType
                        {
                            Code = "MG20",
                            Name = "MG20 File"
                        }
                    },
                    new CaseDocument
                    {
                        DocumentId = "UNUSED 1 - STORM LOG 1881 01.6.20 - EDITED 2020-11-23 MCLOVE",
                        LastUpdatedDate = dt.ToString("s", iv),
                        FileName = "UNUSED 1 - STORM LOG 1881 01.6.20 - EDITED 2020-11-23 MCLOVE.docx",
                        CmsDocType = new CmsDocType
                        {
                            Code = "MG11",
                            Name = "MG11 File"
                        }
                    },
                    new CaseDocument
                    {
                        DocumentId = "Shelagh McLove VPS mg11",
                        LastUpdatedDate = dt.ToString("s", iv),
                        FileName = "Shelagh McLove VPS mg11.docx",
                        CmsDocType = new CmsDocType
                        {
                            Code = "MG11",
                            Name = "MG11 File"
                        }
                    },
                    new CaseDocument
                    {
                        DocumentId = "UNUSED 6 - DA CHECKLIST MCLOVE",
                        LastUpdatedDate = dt.ToString("s", iv),
                        FileName = "UNUSED 6 - DA CHECKLIST MCLOVE.docx",
                        CmsDocType = new CmsDocType
                        {
                            Code = "MG6",
                            Name = "MG6 File"
                        }
                    },
                    new CaseDocument
                    {
                        DocumentId = "MG0",
                        LastUpdatedDate = dt.ToString("s", iv),
                        FileName = "MG0.docx",
                        CmsDocType = new CmsDocType
                        {
                            Code = "MG0",
                            Name = "MG0 File"
                        }
                    },
                    new CaseDocument
                    {
                        DocumentId = "MG06 3 June",
                        LastUpdatedDate = dt.ToString("s", iv),
                        FileName = "MG06 3 June.doc",
                        CmsDocType = new CmsDocType
                        {
                            Code = "MG06",
                            Name = "MG06 File"
                        }
                    },
                    new CaseDocument
                    {
                        DocumentId = "SDC items to be Disclosed (1-6) MCLOVE",
                        LastUpdatedDate = dt.ToString("s", iv),
                        FileName = "SDC items to be Disclosed (1-6) MCLOVE.doc",
                        CmsDocType = new CmsDocType
                        {
                            Code = "MG11",
                            Name = "MG11 File"
                        }
                    },
                    new CaseDocument
                    {
                        DocumentId = "stmt BLAYNEE 2034 1 JUNE mg11",
                        LastUpdatedDate = dt.ToString("s", iv),
                        FileName = "stmt BLAYNEE 2034 1 JUNE mg11.docx",
                        CmsDocType = new CmsDocType
                        {
                            Code = "MG11",
                            Name = "MG11 File"
                        }
                    },
                    new CaseDocument
                    {
                        DocumentId = "PRE CONS D",
                        LastUpdatedDate = dt.ToString("s", iv),
                        FileName = "PRE CONS D.docx",
                        CmsDocType = new CmsDocType
                        {
                            Code = "MG00",
                            Name = "MG00 File"
                        }
                    },
                    new CaseDocument
                    {
                        DocumentId = "MG05 MCLOVE",
                        LastUpdatedDate = dt.ToString("s", iv),
                        FileName = "MG05 MCLOVE.doc",
                        CmsDocType = new CmsDocType
                        {
                            Code = "MG05",
                            Name = "MG05 File"
                        }
                    },
                    new CaseDocument
                    {
                        DocumentId = "MG20 5 JUNE",
                        LastUpdatedDate = dt.ToString("s", iv),
                        FileName = "MG20 5 JUNE.doc",
                        CmsDocType = new CmsDocType
                        {
                            Code = "MG20",
                            Name = "MG20 File"
                        }
                    },
                    new CaseDocument
                    {
                        DocumentId = "MG02 SHELAGH MCLOVE",
                        LastUpdatedDate = dt.ToString("s", iv),
                        FileName = "MG02 SHELAGH MCLOVE.doc",
                        CmsDocType = new CmsDocType
                        {
                            Code = "MG02",
                            Name = "MG02 File"
                        }
                    },
                    new CaseDocument
                    {
                        DocumentId = "MG06 10 june",
                        LastUpdatedDate = dt.ToString("s", iv),
                        FileName = "MG06 10 june.doc",
                        CmsDocType = new CmsDocType
                        {
                            Code = "MG06",
                            Name = "MG06 File"
                        }
                    },
                    new CaseDocument
                    {
                        DocumentId = "stmt Lucy Doyle MG11",
                        LastUpdatedDate = dt.ToString("s", iv),
                        FileName = "stmt Lucy Doyle MG11.docx",
                        CmsDocType = new CmsDocType
                        {
                            Code = "MG11",
                            Name = "MG11 File"
                        }
                    },
                    new CaseDocument
                    {
                        DocumentId = "MCLOVE MG3",
                        LastUpdatedDate = dt.ToString("s", iv),
                        FileName = "MCLOVE MG3.docx",
                        CmsDocType = new CmsDocType
                        {
                            Code = "MG3",
                            Name = "MG3 File"
                        }
                    }
                }
            };
        }

        private static Case MultipleFileTypeCase(string caseId)
        {
            var dt = DateTime.Now;
            var iv = CultureInfo.InvariantCulture;
            return new Case
            {
                CaseId = caseId,
                CaseDocuments = new[]
                {
                    new CaseDocument
                    {
                        DocumentId = "docCDE",
                        LastUpdatedDate = dt.ToString("s", iv),
                        FileName = "docCDE.doc",
                        CmsDocType = new CmsDocType
                        {
                            Code = "MG0",
                            Name = "MG0 File"
                        }
                    },
                    new CaseDocument
                    {
                        DocumentId = "docxCDE",
                        LastUpdatedDate = dt.ToString("s", iv),
                        FileName = "docxCDE.docx",
                        CmsDocType = new CmsDocType
                        {
                            Code = "MG0",
                            Name = "MG0 File"
                        }
                    },
                    new CaseDocument
                    {
                        DocumentId = "docmCDE",
                        LastUpdatedDate = dt.ToString("s", iv),
                        FileName = "docmCDE.docm",
                        CmsDocType = new CmsDocType
                        {
                            Code = "MG0",
                            Name = "MG0 File"
                        }
                    },
                    new CaseDocument
                    {
                        DocumentId = "xlsxCDE",
                        LastUpdatedDate = dt.ToString("s", iv),
                        FileName = "xlsxCDE.xlsx",
                        CmsDocType = new CmsDocType
                        {
                            Code = "MG0",
                            Name = "MG0 File"
                        }
                    },
                    new CaseDocument
                    {
                        DocumentId = "xlsCDE",
                        LastUpdatedDate = dt.ToString("s", iv),
                        FileName = "xlsCDE.xls",
                        CmsDocType = new CmsDocType
                        {
                            Code = "MG0",
                            Name = "MG0 File"
                        }
                    },
                    new CaseDocument
                    {
                        DocumentId = "pptCDE",
                        LastUpdatedDate = dt.ToString("s", iv),
                        FileName = "pptCDE.ppt",
                        CmsDocType = new CmsDocType
                        {
                            Code = "MG0",
                            Name = "MG0 File"
                        }
                    },
                    new CaseDocument
                    {
                        DocumentId = "pptxCDE",
                        LastUpdatedDate = dt.ToString("s", iv),
                        FileName = "pptxCDE.pptx",
                        CmsDocType = new CmsDocType
                        {
                            Code = "MG0",
                            Name = "MG0 File"
                        }
                    },
                    new CaseDocument
                    {
                        DocumentId = "htmlCDE",
                        LastUpdatedDate = dt.ToString("s", iv),
                        FileName = "htmlCDE.html",
                        CmsDocType = new CmsDocType
                        {
                            Code = "MG0",
                            Name = "MG0 File"
                        }
                    },
                    new CaseDocument
                    {
                        DocumentId = "msgCDE",
                        LastUpdatedDate = dt.ToString("s", iv),
                        FileName = "msgCDE.msg",
                        CmsDocType = new CmsDocType
                        {
                            Code = "MG0",
                            Name = "MG0 File"
                        }
                    },
                    new CaseDocument
                    {
                        DocumentId = "vsdCDE",
                        LastUpdatedDate = dt.ToString("s", iv),
                        FileName = "vsdCDE.vsd",
                        CmsDocType = new CmsDocType
                        {
                            Code = "MG0",
                            Name = "MG0 File"
                        }
                    },
                    new CaseDocument
                    {
                        DocumentId = "bmpCDE",
                        LastUpdatedDate = dt.ToString("s", iv),
                        FileName = "bmpCDE.bmp",
                        CmsDocType = new CmsDocType
                        {
                            Code = "MG0",
                            Name = "MG0 File"
                        }
                    },
                    new CaseDocument
                    {
                        DocumentId = "gifCDE",
                        LastUpdatedDate = dt.ToString("s", iv),
                        FileName = "gifCDE.gif",
                        CmsDocType = new CmsDocType
                        {
                            Code = "MG0",
                            Name = "MG0 File"
                        }
                    },
                    new CaseDocument
                    {
                        DocumentId = "jpgCDE",
                        LastUpdatedDate = dt.ToString("s", iv),
                        FileName = "jpgCDE.jpg",
                        CmsDocType = new CmsDocType
                        {
                            Code = "MG0",
                            Name = "MG0 File"
                        }
                    },
                    new CaseDocument
                    {
                        DocumentId = "pngCDE",
                        LastUpdatedDate = dt.ToString("s", iv),
                        FileName = "pngCDE.png",
                        CmsDocType = new CmsDocType
                        {
                            Code = "MG0",
                            Name = "MG0 File"
                        }
                    },
                    new CaseDocument
                    {
                        DocumentId = "tiffCDE",
                        LastUpdatedDate = dt.ToString("s", iv),
                        FileName = "tiffCDE.tiff",
                        CmsDocType = new CmsDocType
                        {
                            Code = "MG0",
                            Name = "MG0 File"
                        }
                    },
                    new CaseDocument
                    {
                        DocumentId = "rtfCDE",
                        LastUpdatedDate = dt.ToString("s", iv),
                        FileName = "rtfCDE.rtf",
                        CmsDocType = new CmsDocType
                        {
                            Code = "MG0",
                            Name = "MG0 File"
                        }
                    },
                    new CaseDocument
                    {
                        DocumentId = "txtCDE",
                        LastUpdatedDate = dt.ToString("s", iv),
                        FileName = "txtCDE.txt",
                        CmsDocType = new CmsDocType
                        {
                            Code = "MG0",
                            Name = "MG0 File"
                        }
                    }
                }
            };
        }
    }
}

