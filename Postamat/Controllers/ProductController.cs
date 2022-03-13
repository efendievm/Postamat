using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Postamat.Models;
using Postamat.Models.Mapping;
using Postamat.Repositories;
using Postamat.Services;
using System.Collections.Generic;
using System.Linq;

namespace Postamat.Controllers
{
    /// <summary>
    /// Контроллер товаров.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        IRepository<Product> Products;
        IRepository<Order> Orders;
        IRepository<CartLine> Lines;
        IOrderPriceCalculator PriceCalculator;
        IMapper Mapper;
        public ProductController(
            IRepository<Product> Products, 
            IRepository<Order> Orders, 
            IRepository<CartLine> Lines, 
            IOrderPriceCalculator PriceCalculator, 
            IMapper Mapper) => 
            (this.Products, this.Orders, this.Lines, this.PriceCalculator, this.Mapper) = (Products, Orders, Lines, PriceCalculator, Mapper);

        /// <summary>
        /// Метод для получения списка названий товаров.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult<IEnumerable<ProductInfoDto>> Get() => Products.GetAll()
            .Select(p => Mapper.Map<ProductInfoDto>(p))
            .AsEnumerable()
            .OrderBy(p => p.Name)
            .ToList();

        /// <summary>
        /// Метод для получения информации по товару по его имени.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        [HttpGet("name/{name}")]
        public ActionResult<ProductInfoDto> Get(string name)
        {
            var product = Products.GetAll().FirstOrDefault(p => p.Name == name);
            if (product == null)
            {
                return NotFound(new { errorText = $"Product {name} not found" });
            }
            return Ok(Mapper.Map<ProductInfoDto>(product));
        }

        /// <summary>
        /// Метод для получения списка товаров.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("all")]
        [Authorize(Roles = "admin")]
        public ActionResult<IEnumerable<Product>> GetAll() => Products.GetAll().ToList();

        /// <summary>
        /// Метод для получения товара по его id.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("{id}")]
        [Authorize(Roles = "admin")]
        public ActionResult<Product> Get(int id)
        {
            var product = Products.Get(id);
            if (product == null)
            {
                return NotFound(new { errorText = $"Product {id} not found" });
            }
            return Ok(product);
        }

        /// <summary>
        /// Метод для создания товара.
        /// </summary>
        /// <param name="product"></param>
        /// <returns></returns>
        [HttpPost]
        [Authorize(Roles = "admin")]
        public ActionResult<Product> Post(Product product)
        {
            if (product == null)
            {
                return BadRequest(new { errorText = "Empty input data." });
            }

            if (Products.GetAll().FirstOrDefault(p => p.Name == product.Name) != null)
            {
                return BadRequest(new { errorText = $"Product with name {product.Name} already exists." });
            }

            var created = Products.Create(product);

            return Ok(created);
        }

        /// <summary>
        /// Метод для редактирования товара.
        /// </summary>
        /// <param name="product"></param>
        /// <returns></returns>
        [HttpPut]
        [Authorize(Roles = "admin")]
        public ActionResult<Product> Put(Product product)
        {
            if (product == null)
            {
                return BadRequest(new { errorText = "Empty input data." });
            }

            if (Products.GetAll().FirstOrDefault(p => (p.Name == product.Name) && (p.ID != product.ID)) != null)
            {
                return BadRequest(new { errorText = $"Product with name {product.Name} already exists." });
            }

            var updated = Products.Update(product);

            if (updated == null)
            {
                return NotFound(new { errorText = $"Product {product.ID} not found." });
            }
            return Ok(updated);
        }

        /// <summary>
        /// Метод для удаления товара.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete("{id}")]
        [Authorize(Roles = "admin")]
        public ActionResult<Product> Delete(int id)
        {
            var product = Products.Get(id);
            if (product == null)
            {
                return NotFound(new { errorText = $"Product {id} not found." });
            }
            // список заказов, содержащих удаляемый товар
            var orderToUpdate = Lines
                .GetAll(line => line.Include(l => l.Product).Include(l => l.Order))
                .Where(l => l.Product.ID == id)
                .Select(l => l.Order.ID)
                .ToList();
            // удаление товара.
            Products.Delete(id);
            // обновление цены заказов, содержащих ранее удалённый товар.
            orderToUpdate.ForEach(orderID =>
            {
                var order = Orders.Get(orderID, order => order
                    .Include(o => o.Postamat)
                    .Include(o => o.Lines)
                    .ThenInclude(l => l.Product)
                    .Include(o => o.Customer));
                order.Price = PriceCalculator.GetPrice(order.Lines);
                Orders.Update(order);
            });
            return Ok(product);
        }
    }
}