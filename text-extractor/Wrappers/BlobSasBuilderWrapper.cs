using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;

namespace text_extractor.Wrappers
{
    public class BlobSasBuilderWrapper : IBlobSasBuilderWrapper
    {
        private BlobSasBuilder _blobSasBuilder;

        public BlobSasBuilderWrapper(BlobSasBuilder blobSasBuilder)
        {
            _blobSasBuilder = blobSasBuilder;
        }

        public BlobSasQueryParameters ToSasQueryParameters(UserDelegationKey userDelegationKey, string accountName)
        {
            return _blobSasBuilder.ToSasQueryParameters(userDelegationKey, accountName);
        }
    }
}
