namespace Requester.Http
{
    public class HttpContentTypes
    {
        public static HttpContentTypes Instance { get; private set; }

        public string ApplicationJavaScript { get { return "application/javascript"; } }
        public string ApplicationJson { get { return "application/json"; } }
        public string ApplicationJsonLd { get { return "application/ld+json"; } }
        public string ApplicationGeoJson { get { return "application/vnd.geo+json"; } }
        public string TextJson { get { return "text/json"; } }

        static HttpContentTypes()
        {
            Instance = new HttpContentTypes();
        }

        private HttpContentTypes() { }
    }
}