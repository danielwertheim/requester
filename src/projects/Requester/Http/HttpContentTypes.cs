namespace Requester.Http
{
    public class HttpContentTypes
    {
        public static HttpContentTypes Instance { get; private set; }

        public string ApplicationJavaScript => "application/javascript";
        public string ApplicationJson => "application/json";
        public string ApplicationJsonLd => "application/ld+json";
        public string ApplicationGeoJson => "application/vnd.geo+json";
        public string ApplicationFormUrlEncoded => "application/x-www-form-urlencoded";
        public string TextJson => "text/json";

        static HttpContentTypes()
        {
            Instance = new HttpContentTypes();
        }

        private HttpContentTypes() { }
    }
}