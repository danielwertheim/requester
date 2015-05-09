namespace Requester.Http
{
    public class HttpHeaders
    {
        public static HttpHeaders Instance { get; private set; }

        public string Accept { get { return "Accept"; } }
        public string Authorization { get { return "Authorization"; } }
        public string IfMatch { get { return "If-Match"; } }

        static HttpHeaders()
        {
            Instance = new HttpHeaders();
        }

        private HttpHeaders() { }
    }
}