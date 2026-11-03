using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RESTmusic.Models;

namespace RESTmusic.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CollectionController : ControllerBase
    {
        private readonly ICollectionRepository repo;

        public CollectionController(ICollectionRepository repo)
        {
            this.repo = repo;
        }

        // GET api/Collection?title=low&artist=niki
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<IEnumerable<Collection>> GetAllCollections(
            [FromQuery] string? title = null, [FromQuery] string? artist = null)
        {
            return Ok(repo.GetAllCollections(title, artist));
        }

        // GET api/Collection/lowkey/NIKI
        [HttpGet("{title}/{artist}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<Collection> GetCollection(string title, string artist)
        {
            Collection? collection = repo.GetCollection(title, artist);
            if (collection == null)
            {
                return NotFound();
            }
            return Ok(collection);
        }

        // POST api/Collection med et musiknummer som JSON i request body.
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public ActionResult<Collection> Post([FromBody] Collection collection)
        {
            if (repo.GetCollection(collection.Title, collection.Artist) != null)
            {
                return Conflict("Et nummer med den titel og kunstner findes allerede.");
            }

            Collection createdCollection = repo.AddCollection(collection);
            // Location-headeren peger på GET-adressen til det nye nummer.
            return CreatedAtAction(nameof(GetCollection),
                new { title = createdCollection.Title, artist = createdCollection.Artist },
                createdCollection);
        }

        // PUT api/Collection/lowkey/NIKI med alle de nye oplysninger som JSON.
        [Authorize(Roles = "Admin")]
        [HttpPut("{title}/{artist}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public ActionResult<Collection> Put(string title, string artist, [FromBody] Collection updatedCollection)
        {
            Collection? collection = repo.GetCollection(title, artist);
            if (collection == null)
            {
                return NotFound();
            }

            // En ændring af titel/kunstner må ikke overskrive et andet nummers nøgle.
            Collection? duplicate = repo.GetCollection(updatedCollection.Title, updatedCollection.Artist);
            if (duplicate != null && duplicate != collection)
            {
                return Conflict("Et nummer med den titel og kunstner findes allerede.");
            }

            Collection? result = repo.UpdateCollection(title, artist, updatedCollection);
            if (result == null)
            {
                return NotFound();
            }
            return Ok(result);
        }

        // DELETE api/Collection/lowkey/NIKI. Kræver et login med rollen Admin.
        [Authorize(Roles = "Admin")]
        [HttpDelete("{title}/{artist}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<Collection> Delete(string title, string artist)
        {
            Collection? deletedCollection = repo.DeleteCollection(title, artist);
            if (deletedCollection == null)
            {
                return NotFound();
            }
            return Ok(deletedCollection);
        }
    }
}
