namespace Requester.Http
{
    public class HttpHeaders
    {
        public static HttpHeaders Instance { get; private set; }

        public string Accept => "Accept";
        public string Authorization => "Authorization";
        public string IfMatch => "If-Match";

        static HttpHeaders()
        {
            Instance = new HttpHeaders();
        }

        private HttpHeaders() { }
    }
}