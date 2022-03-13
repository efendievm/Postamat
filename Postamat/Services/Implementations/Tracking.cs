using Microsoft.EntityFrameworkCore;
using Postamat.Exceptions;
using Postamat.Models;
using Postamat.Models.Mapping;
using Postamat.Repositories;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Postamat.Services
{
    public class Tracking : ITracking
    {
        IOrderPriceCalculator OrderPriceCalculator;
        IRepository<Models.Postamat> Postamats;
        IRepository<Order> Orders;
        IRepository<Product> Products;
        IRepository<CartLine> Lines;
        IRepository<Customer> Customers;

        public Tracking(
            IOrderPriceCalculator OrderPriceCalculator,
            IRepository<Models.Postamat> Postamats,
            IRepository<Order> Orders,
            IRepository<Product> Products,
            IRepository<CartLine> Lines,
            IRepository<Customer> Customers)
        {
            this.OrderPriceCalculator = OrderPriceCalculator;
            this.Postamats = Postamats;
            this.Orders = Orders;
            this.Products = Products;
            this.Lines = Lines;
            this.Customers = Customers;
        }
        
        public Order CreateOrder(OrderInputDto order)
        {
            ValidateCustomer(order.CustomerID, out Customer custmomer);

            ValidatePhoneNumber(order.PhoneNumber);

            ValidatePostamat(order.PostamatNumber, out Models.Postamat postamat);

            ValidateProducts(order.Products, out List<CartLine> lines);

            return Orders.Create(new Order()
            {
                Status = 1,
                Lines = lines,
                Price = OrderPriceCalculator.GetPrice(lines),
                Postamat = postamat,
                Name = order.CustomerName,
                PhoneNumber = order.PhoneNumber,
                Customer = custmomer
            });
        }

        public Order UpdateOrder(OrderInputDto order)
        {
            ValidateOrder(order.ID, out Order OrderToUpdate);
            
            ValidatePhoneNumber(order.PhoneNumber);

            ValidateProducts(order.Products, out List<CartLine> lines);

            Lines.GetAll(line => line.Include(l => l.Order)).Where(l => l.Order.ID == order.ID).Select(l => l.ID).ToList().ForEach(l => Lines.Delete(l));
            OrderToUpdate.Lines = lines;
            OrderToUpdate.Price = OrderPriceCalculator.GetPrice(lines);
            OrderToUpdate.Name = order.CustomerName;
            OrderToUpdate.PhoneNumber = order.PhoneNumber;

            return Orders.Update(OrderToUpdate);
        }

        public Order UpdateOrder(int id, string postamatNumber, int status)
        {
            ValidateOrder(id, out Order OrderToUpdate);

            ValidatePostamat(postamatNumber, out Models.Postamat postamat);

            OrderToUpdate.Status = status;
            OrderToUpdate.Postamat = postamat;
           
            return Orders.Update(OrderToUpdate);
        }

        public Order CancelOrder(int id)
        {
            ValidateOrder(id, out Order OrderToCancel);

            OrderToCancel.Status = 6;
            return Orders.Update(OrderToCancel);
        }

        void ValidatePhoneNumber(string PhoneNumber)
        {
            if (!Regex.IsMatch(PhoneNumber, @"^\+7\d{3}-\d{3}-\d{2}-\d{2}$"))
            {
                throw new IncorrectPhoneNumber(PhoneNumber);
            }
        }

        void ValidatePostamat(string PostamatNumber, out Models.Postamat postamat)
        {
            if (!Regex.IsMatch(PostamatNumber, @"^\d{4}-\d{3}$"))
            {
                throw new IncorrectPostamatNumber(PostamatNumber);
            }
            
            postamat = Postamats.GetAll().FirstOrDefault(p => p.Number == PostamatNumber);
            if (postamat == null)
            {
                throw new PostamatNotFound(PostamatNumber);
            }

            if (!postamat.IsWorking)
            {
                throw new PostamatClosed(PostamatNumber);
            }
        }

        void ValidateProducts(List<string> ProductsName, out List<CartLine> Lines)
        {
            if (ProductsName.Count > 10)
            {
                throw new ProductsCountExceeded(ProductsName.Count);
            }

            Lines = new List<CartLine>();
            foreach (var line in ProductsName.GroupBy(p => p).Select(p => new { Name = p.Key, Quantity = p.Count()}))
            {
                var product = Products.GetAll().FirstOrDefault(p => p.Name == line.Name);
                if (product == null)
                {
                    throw new ProductNotFound(line.Name);
                }
                Lines.Add(new CartLine { Product = product, Quantity = line.Quantity});
            }
        }

        void ValidateOrder(int id, out Order order)
        {
            order = Orders.Get(id, orders => orders.Include(o => o.Postamat).Include(o => o.Customer));
            if (order == null)
            {
                throw new OrderNotFound(id);
            }
        }

        void ValidateCustomer(int id, out Customer custmomer)
        {
            custmomer = Customers.Get(id);
            if (custmomer == null)
            {
                throw new CustomerNotFound(id);
            }
        }
    }
}