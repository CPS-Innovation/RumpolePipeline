using System;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using Common.Logging;
using Microsoft.Extensions.Logging;

namespace text_extractor.Wrappers
{
    public class BlobSasBuilderWrapper : IBlobSasBuilderWrapper
    {
        private readonly BlobSasBuilder _blobSasBuilder;
        
        public BlobSasBuilderWrapper(BlobSasBuilder blobSasBuilder)
        {
            _blobSasBuilder = blobSasBuilder;
        }

        public BlobSasQueryParameters ToSasQueryParameters(UserDelegationKey userDelegationKey, string accountName, Guid correlationId)
        {
            var result = _blobSasBuilder.ToSasQueryParameters(userDelegationKey, accountName);
            return result;
        }
    }
}
