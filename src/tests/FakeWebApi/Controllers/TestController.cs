using FakeWebApi.Model;
using Microsoft.AspNetCore.Mvc;

namespace FakeWebApi.Controllers
{
    [Route("api/test")]
    public class TestController : Controller
    {
        [HttpGet("nocontent")]
        public IActionResult GetNoContent() => NoContent();

        [HttpGet("null")]
        public IActionResult GetNull() => Ok(null as Person);

        [HttpGet("empty")]
        public IActionResult GetEmpty() => Ok();
    }
}