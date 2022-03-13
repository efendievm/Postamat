using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Postamat.Exceptions;
using Postamat.Models;
using Postamat.Models.Mapping;
using Postamat.Repositories;
using Postamat.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;


namespace Postamat.Controllers
{
    /// <summary>
    /// Контроллер заказов.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        ITracking Tracking;
        IRepository<Order> Orders;
        IRepository<User> Users;
        IMapper Mapper;

        public OrderController(ITracking Tracking, IRepository<Order> Orders, IRepository<User> Users, IMapper Mapper)
        {
            this.Tracking = Tracking;
            this.Orders = Orders;
            this.Users = Users;
            this.Mapper = Mapper;
        }

        /// <summary>
        /// Метод, получающий ID текущего пользователя.
        /// </summary>
        /// <returns></returns>
        int CurrentCustomerID()
        {
            var login = ((ClaimsIdentity)User.Identity).Claims.Where(c => c.Type == ClaimTypes.Name).Select(c => c.Value).FirstOrDefault();
            return Users.GetAll().FirstOrDefault(u => u.Login == login).CustomerID;
        }

        /// <summary>
        /// Метод, получающий все заказы пользователя.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Authorize(Roles = "customer")]
        public ActionResult<IEnumerable<OrderInfoDto>> Get() => Ok(Orders
            .GetAll(order => order.Include(o => o.Customer).Include(o => o.Lines).ThenInclude(l => l.Product).Include(o => o.Postamat))
            /// здесь выбираем только заказы текущего пользователя
            .Where(o => o.Customer.ID == CurrentCustomerID())
            .Select(o => Mapper.Map<OrderInfoDto>(o))
            .ToList());

        /// <summary>
        /// Метод, возвращающий заказ по его id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}")]
        [Authorize(Roles = "customer")]
        public ActionResult<OrderInfoDto> Get(int id)
        {
            var order = Orders.Get(id, order => order.Include(o => o.Customer).Include(o => o.Lines).ThenInclude(l => l.Product).Include(o => o.Postamat));
            // если пользователь запросил не свои заказы, выдаём код ошибки 
            if (order.Customer.ID != CurrentCustomerID())
            {
                return Unauthorized(new { errorText = "Invalid username or password." });
            }
            if (order == null)
            {
                return NotFound(new { errorText = $"Order {id} not found." });
            }

            return Ok(Mapper.Map<OrderInfoDto>(order));
        }

        /// <summary>
        /// Метод для создания заказа.
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        [HttpPost]
        [Authorize(Roles = "customer")]
        public ActionResult<OrderInfoDto> Post(OrderInputDto order)
        {
            if (order == null)
            {
                return BadRequest(new { errorText = "Empty input data." });
            }
            // получаем id текущего пользователя
            order.CustomerID = CurrentCustomerID();
            try
            {
                return Ok(Mapper.Map<OrderInfoDto>(Tracking.CreateOrder(order)));
            }
            catch(Exception ex)
            { 
                if (ex is IncorrectPhoneNumber || ex is IncorrectPostamatNumber || ex is ProductsCountExceeded)
                {
                    return BadRequest(new { errorText = ex.Message });
                }
                else if (ex is PostamatNotFound || ex is ProductNotFound || ex is CustomerNotFound)
                {
                    return NotFound(new { errorText = ex.Message });
                }
                else if (ex is PostamatClosed)
                {
                    return Forbid();
                }
                return BadRequest(new { errorText = ex.Message });
            }
        }

        /// <summary>
        /// Метод для редактирования заказа.
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        [HttpPut]
        [Authorize(Roles = "customer")]
        public ActionResult<OrderInfoDto> Put(OrderInputDto order)
        {
            if (order == null)
            {
                return BadRequest(new { errorText = "Empty input data." });
            }
            // если пользователь пытается изменить не свой заказ, выдаём код ошибки
            if (Orders.Get(order.ID, orders => orders.Include(o => o.Customer)).Customer.ID != CurrentCustomerID())
            {
                return Unauthorized();
            }
            try
            {
                return Ok(Mapper.Map<OrderInfoDto>(Tracking.UpdateOrder(order)));
            }
            catch (Exception ex)
            {
                if (ex is IncorrectPhoneNumber || ex is ProductsCountExceeded)
                {
                    return BadRequest(new { errorText = ex.Message });
                }
                else if (ex is OrderNotFound || ex is ProductNotFound || ex is CustomerNotFound)
                {
                    return NotFound(new { errorText = ex.Message });
                }
                return BadRequest(new { errorText = ex.Message });
            }
        }

        /// <summary>
        /// Метод для отмены заказа по id заказа.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPut("cancel/{id}")]
        [Authorize(Roles = "customer")]
        public ActionResult<OrderInfoDto> Cancel(int id)
        {
            // если пользователь пытается отменить не свой заказ, выдаём код ошибки.
            if (Orders.Get(id, orders => orders.Include(o => o.Customer)).Customer.ID != CurrentCustomerID())
            {
                return Unauthorized();
            }
            try
            {
                return Ok(Mapper.Map<OrderInfoDto>(Tracking.CancelOrder(id)));
            }
            catch (ProductNotFound ex)
            {
                return NotFound(new { errorText = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { errorText = ex.Message });
            }
        }

        /// <summary>
        /// Метод для получения всех заказов.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("all")]
        [Authorize(Roles = "admin")]
        public ActionResult<IEnumerable<OrderAdminInfoDto>> All() => Orders
            .GetAll(order => order.Include(o => o.Customer).Include(o => o.Lines).ThenInclude(l => l.Product).Include(o => o.Postamat))
            .Select(o => Mapper.Map<OrderAdminInfoDto>(o))
            .ToList();
        
        /// <summary>
        /// Метод для получения заказа по id заказа.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("id/{id}")]
        [Authorize(Roles = "admin")]
        public ActionResult<OrderAdminInfoDto> GetByID(int id)
        {
            var order = Orders.Get(id, order => order.Include(o => o.Customer).Include(o => o.Lines).ThenInclude(l => l.Product).Include(o => o.Postamat));
            if (order == null)
            {
                return NotFound(new { errorText = $"Order {id} not found" });
            }

            return Ok(Mapper.Map<OrderAdminInfoDto>(order));
        }

        /// <summary>
        /// Метод для изменения статуса заказа.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="postamatNumber"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        [HttpPut("{id}/postamat/{postamatNumber}/status/{status}")]
        [Authorize(Roles = "admin")]
        public ActionResult<OrderAdminInfoDto> Put(int id, string postamatNumber, int status)
        {
            try
            {
                return Ok(Mapper.Map<OrderAdminInfoDto>(Tracking.UpdateOrder(id, postamatNumber, status)));
            }
            catch (Exception ex)
            {
                if (ex is IncorrectPostamatNumber)
                {
                    return BadRequest(new { errorText = ex.Message });
                }
                else if (ex is OrderNotFound || ex is PostamatNotFound)
                {
                    return NotFound(new { errorText = ex.Message });
                }
                throw;
            }
        }

        
        /// <summary>
        /// Метод для удаления заказа.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete("{id}")]
        [Authorize(Roles = "admin")]
        public ActionResult<OrderAdminInfoDto> Delete(int id)
        {
            var order = Orders.Get(id);
            if (order == null)
            {
                return NotFound(new { errorText = $"Order {id} not found" });
            }

            var deleted = Orders.Delete(id);

            return Ok(Mapper.Map<OrderAdminInfoDto>(deleted));
        }
    }
}