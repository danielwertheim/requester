using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace Requester.Http
{
    public class JsonContent : StringContent
    {
        public JsonContent() : this(string.Empty) { }
        public JsonContent(string content) : base(content, Encoding.UTF8, HttpContentTypes.Instance.ApplicationJson) {}
    }

    public class BytesContent : ByteArrayContent
    {
        public BytesContent(byte[] content, string contentType)
            : base(content)
        {
            Headers.ContentType = new MediaTypeHeaderValue(contentType);
        }
    }
}