using FakeWebApi.Model;
using Microsoft.AspNetCore.Mvc;

namespace FakeWebApi.Controllers
{
    [Route("api/relay")]
    public class RelayController : Controller
    {
        [HttpPost]
        public IActionResult Post([FromForm]Item model) => StatusCode(200, model);

        [HttpPut]
        public IActionResult Put([FromForm]Item model) => StatusCode(200, model);
    }
}