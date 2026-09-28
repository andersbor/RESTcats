using Microsoft.AspNetCore.Mvc;
using RESTcats.Models;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace RESTcats.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CatsController : ControllerBase
    {
        private readonly ICatsRepository catsRepository;

        public CatsController(ICatsRepository catsRepository)
        {
            this.catsRepository = catsRepository;
        }

        // GET: api/<CatsController>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<IEnumerable<Cat>> Get()
        {
            IEnumerable<Cat> cats = catsRepository.GetAllCats();
            return Ok(cats);
        }

        // GET api/<CatsController>/5
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<Cat> Get(int id)
        {
            Cat? cat = catsRepository.GetCatById(id);
            if (cat == null)
            {
                return NotFound();
            }
            return Ok(cat);
        }

        // POST api/<CatsController>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        public ActionResult<Cat> Post([FromBody] Cat cat)
        {
            catsRepository.AddCat(cat);
            return CreatedAtAction(nameof(Get), new { id = cat.Id }, cat);
        }

        // PUT api/<CatsController>/5
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<Cat> Put(int id, [FromBody] Cat cat)
        {
            Cat? updatedCat = catsRepository.UpdateCat(id, cat);
            if (updatedCat == null)
            {
                return NotFound();
            }
            else
            {
                return Ok(updatedCat);
            }
        }

        

        // DELETE api/<CatsController>/5
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<Cat> Delete(int id)
        {
            Cat? cat = catsRepository.GetCatById(id);
            if (cat == null)
            {
                return NotFound();
            }
            catsRepository.RemoveCat(id);
            return Ok(cat);
        }
    }
}
