using System.Net.Http;
using System.Web.Http;
using Requester.TestWebApi.Model;

namespace Requester.TestWebApi.Controllers
{
    [RoutePrefix("api/relay")]
    public class RelayController : ApiController
    {
        [HttpPost]
        [Route]
        public HttpResponseMessage Post(Item model)
        {
            return Request.CreateResponse(model);
        }

        [HttpPut]
        [Route]
        public HttpResponseMessage Put(Item model)
        {
            return Request.CreateResponse(model);
        }
    }
}