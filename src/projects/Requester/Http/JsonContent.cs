using System.Net.Http;
using System.Text;

namespace Requester.Http
{
    public class JsonContent : StringContent
    {
        public JsonContent() : this(string.Empty) { }
        public JsonContent(string content) : base(content, Encoding.UTF8, HttpContentTypes.Instance.ApplicationJson) {}
    }
}