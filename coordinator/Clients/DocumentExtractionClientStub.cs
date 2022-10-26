using System;
using System.Threading.Tasks;
using Common.Domain.DocumentExtraction;
using Common.Domain.Extensions;
using Common.Logging;
using Microsoft.Extensions.Logging;

namespace coordinator.Clients
{
	public class DocumentExtractionClientStub : IDocumentExtractionClient
    {
        private readonly ILogger<DocumentExtractionClientStub> _logger;

        public DocumentExtractionClientStub(ILogger<DocumentExtractionClientStub> logger)
        {
            _logger = logger;
        }

        public Task<Case> GetCaseDocumentsAsync(string caseId, string accessToken, Guid correlationId)
        {
            _logger.LogMethodEntry(correlationId, nameof(GetCaseDocumentsAsync), caseId);
            
            var result = Task.FromResult(caseId switch
            {
                "18846" => McLoveCase(caseId),
                "1000000" => McLoveCase(caseId),
                "18848" => MultipleFileTypeCase(caseId),
                _ => null
            });
            
            _logger.LogMethodExit(correlationId, nameof(GetCaseDocumentsAsync), result.ToJson());
            return result;
        }

        private static Case McLoveCase(string caseId)
        {
            return new Case
            {
                CaseId = caseId,
                CaseDocuments = new[]
                {
                    new CaseDocument
                    {
                        DocumentId = "MG12",
                        MaterialId = null,
                        LastUpdatedDate = DateTime.Now.ToString("yyyy-MM-dd"),
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
                        MaterialId = null,
                        LastUpdatedDate = DateTime.Now.ToString("yyyy-MM-dd"),
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
                        MaterialId = null,
                        LastUpdatedDate = DateTime.Now.ToString("yyyy-MM-dd"),
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
                        MaterialId = null,
                        LastUpdatedDate = DateTime.Now.ToString("yyyy-MM-dd"),
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
                        MaterialId = null,
                        LastUpdatedDate = DateTime.Now.ToString("yyyy-MM-dd"),
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
                        MaterialId = null,
                        LastUpdatedDate = DateTime.Now.ToString("yyyy-MM-dd"),
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
                        MaterialId = null,
                        LastUpdatedDate = DateTime.Now.ToString("yyyy-MM-dd"),
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
                        MaterialId = null,
                        LastUpdatedDate = DateTime.Now.ToString("yyyy-MM-dd"),
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
                        MaterialId = null,
                        LastUpdatedDate = DateTime.Now.ToString("yyyy-MM-dd"),
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
                        MaterialId = null,
                        LastUpdatedDate = DateTime.Now.ToString("yyyy-MM-dd"),
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
                        MaterialId = null,
                        LastUpdatedDate = DateTime.Now.ToString("yyyy-MM-dd"),
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
                        MaterialId = null,
                        LastUpdatedDate = DateTime.Now.ToString("yyyy-MM-dd"),
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
                        MaterialId = null,
                        LastUpdatedDate = DateTime.Now.ToString("yyyy-MM-dd"),
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
                        MaterialId = null,
                        LastUpdatedDate = DateTime.Now.ToString("yyyy-MM-dd"),
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
                        MaterialId = null,
                        LastUpdatedDate = DateTime.Now.ToString("yyyy-MM-dd"),
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
                        MaterialId = null,
                        LastUpdatedDate = DateTime.Now.ToString("yyyy-MM-dd"),
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
                        MaterialId = null,
                        LastUpdatedDate = DateTime.Now.ToString("yyyy-MM-dd"),
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
                        MaterialId = null,
                        LastUpdatedDate = DateTime.Now.ToString("yyyy-MM-dd"),
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
                        MaterialId = null,
                        LastUpdatedDate = DateTime.Now.ToString("yyyy-MM-dd"),
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
            return new Case
            {
                CaseId = caseId,
                CaseDocuments = new[]
                {
                    new CaseDocument
                    {
                        DocumentId = "docCDE",
                        MaterialId = null,
                        LastUpdatedDate = DateTime.Now.ToString("yyyy-MM-dd"),
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
                        MaterialId = null,
                        LastUpdatedDate = DateTime.Now.ToString("yyyy-MM-dd"),
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
                        MaterialId = null,
                        LastUpdatedDate = DateTime.Now.ToString("yyyy-MM-dd"),
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
                        MaterialId = null,
                        LastUpdatedDate = DateTime.Now.ToString("yyyy-MM-dd"),
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
                        MaterialId = null,
                        LastUpdatedDate = DateTime.Now.ToString("yyyy-MM-dd"),
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
                        MaterialId = null,
                        LastUpdatedDate = DateTime.Now.ToString("yyyy-MM-dd"),
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
                        MaterialId = null,
                        LastUpdatedDate = DateTime.Now.ToString("yyyy-MM-dd"),
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
                        MaterialId = null,
                        LastUpdatedDate = DateTime.Now.ToString("yyyy-MM-dd"),
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
                        MaterialId = null,
                        LastUpdatedDate = DateTime.Now.ToString("yyyy-MM-dd"),
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
                        MaterialId = null,
                        LastUpdatedDate = DateTime.Now.ToString("yyyy-MM-dd"),
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
                        MaterialId = null,
                        LastUpdatedDate = DateTime.Now.ToString("yyyy-MM-dd"),
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
                        MaterialId = null,
                        LastUpdatedDate = DateTime.Now.ToString("yyyy-MM-dd"),
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
                        MaterialId = null,
                        LastUpdatedDate = DateTime.Now.ToString("yyyy-MM-dd"),
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
                        MaterialId = null,
                        LastUpdatedDate = DateTime.Now.ToString("yyyy-MM-dd"),
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
                        MaterialId = null,
                        LastUpdatedDate = DateTime.Now.ToString("yyyy-MM-dd"),
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
                        MaterialId = null,
                        LastUpdatedDate = DateTime.Now.ToString("yyyy-MM-dd"),
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
                        MaterialId = null,
                        LastUpdatedDate = DateTime.Now.ToString("yyyy-MM-dd"),
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

