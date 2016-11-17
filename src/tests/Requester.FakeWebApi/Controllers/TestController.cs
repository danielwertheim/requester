using System.Net;
using System.Net.Http;
using System.Web.Http;
using Requester.FakeWebApi.Model;

namespace Requester.FakeWebApi.Controllers
{
    [RoutePrefix("api/test")]
    public class TestController : ApiController
    {
        [HttpGet]
        [Route("nocontent")]
        public HttpResponseMessage GetNoContent()
        {
            return Request.CreateResponse(HttpStatusCode.NoContent);
        }

        [HttpGet]
        [Route("null")]
        public HttpResponseMessage GetNull()
        {
            return Request.CreateResponse(HttpStatusCode.OK, null as Person);
        }

        [HttpGet]
        [Route("empty")]
        public HttpResponseMessage GetEmpty()
        {
            return Request.CreateResponse(HttpStatusCode.OK);
        }
    }
}