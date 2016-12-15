namespace Requester.Http
{
    public class HttpRequesterHeaders
    {
        public static HttpRequesterHeaders Instance { get; private set; }

        public string Accept => "Accept";
        public string Authorization => "Authorization";
        public string IfMatch => "If-Match";
        public string IfNoneMatch => "If-None-Match";

        static HttpRequesterHeaders()
        {
            Instance = new HttpRequesterHeaders();
        }

        private HttpRequesterHeaders() { }
    }
}