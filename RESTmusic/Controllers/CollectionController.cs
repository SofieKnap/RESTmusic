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

        // GET api/Collection/1
        [HttpGet("{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<Collection> GetCollection(int id)
        {
            Collection? collection = repo.GetCollection(id);
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
            if (repo.GetAllCollections(collection.Title, collection.Artist)
                .Any(c => c.Title == collection.Title && c.Artist == collection.Artist))
            {
                return Conflict("Et nummer med den titel og kunstner findes allerede.");
            }

            Collection createdCollection = repo.AddCollection(collection);
            // Location-headeren peger på GET-adressen til det nye nummer.
            return CreatedAtAction(nameof(GetCollection),
                new { id = createdCollection.Id },
                createdCollection);
        }

        // PUT api/Collection/1 med alle de nye oplysninger som JSON.
        [Authorize(Roles = "Admin")]
        [HttpPut("{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public ActionResult<Collection> Put(int id, [FromBody] Collection updatedCollection)
        {
            Collection? collection = repo.GetCollection(id);
            if (collection == null)
            {
                return NotFound();
            }

            // Titel og kunstner kan ændres. Id'et forbliver det samme.
            bool duplicate = repo.GetAllCollections(updatedCollection.Title, updatedCollection.Artist)
                .Any(c => c.Id != id && c.Title == updatedCollection.Title && c.Artist == updatedCollection.Artist);
            if (duplicate)
            {
                return Conflict("Et nummer med den titel og kunstner findes allerede.");
            }

            Collection? result = repo.UpdateCollection(id, updatedCollection);
            if (result == null)
            {
                return NotFound();
            }
            return Ok(result);
        }

        // DELETE api/Collection/1. Kræver et login med rollen Admin.
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<Collection> Delete(int id)
        {
            Collection? deletedCollection = repo.DeleteCollection(id);
            if (deletedCollection == null)
            {
                return NotFound();
            }
            return Ok(deletedCollection);
        }
    }
}
