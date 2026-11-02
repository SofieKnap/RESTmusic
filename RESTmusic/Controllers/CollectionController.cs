using Microsoft.AspNetCore.Mvc;
using RESTmusic.Models;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace RESTmusic.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CollectionController : ControllerBase
    {

        private ICollectionRepository repo;
        public CollectionController(ICollectionRepository repo)
        {
            this.repo = repo;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<IEnumerable<Collection>> GetAllCollections(
                [FromQuery] string? title = null, [FromQuery] string? artist = null)
        {
            return Ok(repo.GetAllCollections(title, artist));
        }



        // GET: api/<CollectionController>
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/<CollectionController>/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/<CollectionController>
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT api/<CollectionController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<CollectionController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
