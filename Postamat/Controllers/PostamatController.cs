using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using Postamat.Repositories;
using Postamat.Models.Mapping;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;

namespace Postamat.Controllers
{
    /// <summary>
    /// Контроллер постаматов.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class PostamatController : ControllerBase
    {
        IRepository<Models.Postamat> Postamats;
        IMapper Mapper;
        public PostamatController(IRepository<Models.Postamat> Postamats, IMapper Mapper) => (this.Postamats, this.Mapper) = (Postamats, Mapper);

        /// <summary>
        /// Метод для получения списка номеров всех постаматов.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult<IEnumerable<string>> Get() => Postamats.GetAll()
            .Where(p => p.IsWorking)
            .Select(p => p.Number)
            .OrderBy(p => p)
            .ToList();


        /// <summary>
        /// Метод для получения информации по постамату по его номеру.
        /// </summary>
        /// <param name="number"></param>
        /// <returns></returns>
        [HttpGet("number/{number}")]
        public ActionResult<PostamatInfoDto> Get(string number)
        {
            var postamat = Postamats.GetAll().FirstOrDefault(p => p.Number == number);
            if (postamat == null)
            {
                return NotFound(new { errorText = $"Postamat {number} not found" });
            }
            return Ok(Mapper.Map<PostamatInfoDto>(postamat));
        }
        
        /// <summary>
        /// Метод для получения списка всех постаматов.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("all")]
        [Authorize(Roles = "admin")]
        public ActionResult<IEnumerable<Models.Postamat>> GetAll() => Postamats.GetAll().ToList();

        /// <summary>
        /// Метод для получения постамата по id.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("{id}")]
        [Authorize(Roles = "admin")]
        public ActionResult<Models.Postamat> Get(int id)
        {
            var postamat = Postamats.Get(id);
            if (postamat == null)
            {
                return NotFound(new { errorText = $"Postamat {id} not found" });
            }
            return Ok(postamat);
        }

        /// <summary>
        /// Метод для изменения создания постамата.
        /// </summary>
        /// <param name="postamat"></param>
        /// <returns></returns>
        [HttpPost]
        [Authorize(Roles = "admin")]
        public ActionResult<Models.Postamat> Post(Models.Postamat postamat)
        {
            if (postamat == null)
            {
                return BadRequest(new { errorText = $"Empty input data" });
            }

            if (Postamats.GetAll().FirstOrDefault(p => p.Number == postamat.Number) != null)
            {
                return BadRequest(new { errorText = $"Postamat with number {postamat.Number} already exists." });
            }

            var created = Postamats.Create(postamat);
            
            return Ok(created);
        }

        /// <summary>
        /// Метод для изменения постамата.
        /// </summary>
        /// <param name="postamat"></param>
        /// <returns></returns>
        [HttpPut]
        [Authorize(Roles = "admin")]
        public ActionResult<Models.Postamat> Put(Models.Postamat postamat)
        {
            if (Postamats.GetAll().FirstOrDefault(p => (p.Number == postamat.Number) && (p.ID != postamat.ID)) != null)
            {
                return BadRequest(new { errorText = $"Postamat with number {postamat.Number} already exists." });
            }
            var updated = Postamats.Update(postamat);
            if (updated == null)
            {
                return NotFound(new { errorText = $"Postamat {postamat.ID} not found" });
            }
            return Ok(updated);
        }

        /// <summary>
        /// Метод для удаления постамата.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete("{id}")]
        [Authorize(Roles = "admin")]
        public ActionResult<Models.Postamat> Delete(int id)
        {
            var deleted = Postamats.Delete(id);
            if (deleted == null)
            {
                return NotFound(new { errorText = $"Postamat {id} not found" });
            }
            return Ok(deleted);
        }
    }
}