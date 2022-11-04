using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Threading.Tasks;
using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Common.Constants;
using Common.Domain.DocumentExtraction;
using Common.Logging;
using Microsoft.Extensions.Logging;
using static System.IO.Path;

namespace coordinator.TempService
{
    public class BlobStorageService : IBlobStorageService
    {
        private readonly string _blobStorageConnectionString;
        private readonly ILogger<BlobStorageService> _logger;
        private readonly string _blobContainerName;
        
        public BlobStorageService(string blobStorageConnectionString, ILogger<BlobStorageService> logger, string blobContainerName)
        {
            _blobStorageConnectionString = blobStorageConnectionString;
            _logger = logger;
            _blobContainerName = blobContainerName;
        }

        public async Task<Case> GetDocumentsAsync(string caseId, Guid correlationId)
        {
            _logger.LogMethodEntry(correlationId, nameof(GetDocumentsAsync), caseId);
            var result = new Case {CaseId = caseId};
            var caseDocuments = new List<CaseDocument>();

            var blobContainerClient = new BlobContainerClient(_blobStorageConnectionString, _blobContainerName);
            if (!await blobContainerClient.ExistsAsync())
                throw new RequestFailedException((int)HttpStatusCode.NotFound, $"Blob container 'cms-documents-2' does not exist");

            var delimiter = string.Concat(caseId, "/");
            var blobs = blobContainerClient.GetBlobsByHierarchyAsync(BlobTraits.Metadata, BlobStates.All, delimiter, delimiter);
            await foreach (var blobHierarchyItem in blobs)
            {
                if (!blobHierarchyItem.IsBlob) continue;
                
                var caseDocument = new CaseDocument
                {
                    DocumentId = GetFileNameWithoutExtension(blobHierarchyItem.Blob.Name),
                    FileName = blobHierarchyItem.Blob.Name,
                    LastUpdatedDate = DeriveLastUpdateDate(blobHierarchyItem.Blob.Metadata, blobHierarchyItem.Blob.Properties.CreatedOn.GetValueOrDefault(DateTimeOffset.Now))
                };
                caseDocument.CmsDocType = InferDocType(caseDocument.DocumentId);

                caseDocuments.Add(caseDocument);
            }

            result.CaseDocuments = caseDocuments.ToArray();
            
            _logger.LogMethodExit(correlationId, nameof(GetDocumentsAsync), string.Empty);
            return result;
        }

        private CmsDocType InferDocType(string documentId)
        {
            switch (documentId)
            {
                case "MG12":
                    return new CmsDocType {Code = "MG12", Name = "MG12 File"};
                case "stmt Shelagh McLove MG11":
                    return new CmsDocType {Code = "MG11", Name = "MG11 File"};
                case "MG00":
                    return new CmsDocType {Code = "MG00", Name = "MG00 File"};
                case "stmt JONES 1989 1 JUNE mg11":
                    return new CmsDocType {Code = "MG11", Name = "MG11 File"};
                case "MG20 10 JUNE":
                    return new CmsDocType {Code = "MG20", Name = "MG20 File"};
                case "UNUSED 1 - STORM LOG 1881 01.6.20 - EDITED 2020-11-23 MCLOVE":
                    return new CmsDocType {Code = "MG11", Name = "MG11 File"};
                case "Shelagh McLove VPS mg11":
                    return new CmsDocType {Code = "MG11", Name = "MG11 File"};
                case "UNUSED 6 - DA CHECKLIST MCLOVE":
                    return new CmsDocType {Code = "MG6", Name = "MG6 File"};
                case "MG0":
                    return new CmsDocType {Code = "MG0", Name = "MG0 File"};
                case "MG06 3 June":
                    return new CmsDocType {Code = "MG06", Name = "MG06 File"};
                case "SDC items to be Disclosed (1-6) MCLOVE":
                    return new CmsDocType {Code = "MG11", Name = "MG11 File"};
                case "stmt BLAYNEE 2034 1 JUNE mg11":
                    return new CmsDocType {Code = "MG11", Name = "MG11 File"};
                case "PRE CONS D":
                    return new CmsDocType {Code = "MG00", Name = "MG00 File"};
                case "MG05 MCLOVE":
                    return new CmsDocType {Code = "MG05", Name = "MG05 File"};
                case "MG20 5 JUNE":
                    return new CmsDocType {Code = "MG20", Name = "MG20 File"};
                case "MG02 SHELAGH MCLOVE":
                    return new CmsDocType {Code = "MG02", Name = "MG02 File"};
                case "MG06 10 june":
                    return new CmsDocType {Code = "MG06", Name = "MG06 File"};
                case "stmt Lucy Doyle MG11":
                    return new CmsDocType {Code = "MG11", Name = "MG11 File"};
                case "MCLOVE MG3":
                    return new CmsDocType {Code = "MG3", Name = "MG3 File"};
                default:
                    return new CmsDocType {Code = "MG0", Name = "MG0 File"};
            }
        }

        private static string DeriveLastUpdateDate(IDictionary<string, string> blobMetaData, DateTimeOffset createdOn)
        {
            var result = string.Empty;
            using var enumerator = blobMetaData.GetEnumerator();
            while (enumerator.MoveNext())
            {
                var currentMetaData = enumerator.Current;
                if (!currentMetaData.Key.Equals(DocumentTags.LastUpdatedDate)) continue;
                
                result = currentMetaData.Value;
                break;
            }

            if (!string.IsNullOrEmpty(result)) return result;
            
            var iv = CultureInfo.InvariantCulture;
            result = createdOn.ToString("s", iv);
            return result;
        }
    }
}