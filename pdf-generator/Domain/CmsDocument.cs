using System.IO;
using System.Net.Http.Headers;

namespace Domain
{
    public class CmsDocument
    {
        public MediaTypeHeaderValue ContentType { get; set; }
        public Stream Stream { get; set; }
    }
}