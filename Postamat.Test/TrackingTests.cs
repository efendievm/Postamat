using Moq;
using Postamat.Models;
using Postamat.Repositories;
using Postamat.Services;
using Postamat.Models.Mapping;
using System;
using System.Linq;
using Xunit;
using Postamat.Exceptions;
using Microsoft.EntityFrameworkCore.Query;
using System.Collections.Generic;

namespace Postamat.Test
{
    public class TrackingTests
    {
        [Fact]
        public void CanThrowCustomerNotFoundExceptionWhenOrderCreated()
        {
            // Arrange
            var mock = new Mock<IRepository<Customer>>();
            mock.Setup(m => m.Get(It.IsAny<int>())).Returns((int id) => (new Customer[]
            {
                new Customer { ID = 1 },
                new Customer { ID = 2 }
            }).FirstOrDefault(c => c.ID == id));

            ITracking tracking = new Tracking(OrderPriceCalculator: null, Postamats: null, Orders: null, Products: null, Lines: null, Customers: mock.Object);

            //Act
            Action act = () => tracking.CreateOrder(new OrderInputDto { CustomerID = 3 });

            //Assert
            Assert.Throws<CustomerNotFound>(() => act());
        }

        [Theory]
        [InlineData("+7(999)999-99-99", true)]
        [InlineData("+7-999-999-99-99", true)]
        [InlineData("8-999-999-99-99", true)]
        [InlineData("89999999999", true)]
        [InlineData("+7999-999-99-99", false)]
        public void CanThrowIncorrectPhoneNumberExceptionWhenOrderCreated(string phoneNumber, bool expectsException)
        {
            // Arrange
            var mock = new Mock<IRepository<Customer>>();
            mock.Setup(m => m.Get(It.IsAny<int>())).Returns((int id) => new Customer());
            
            ITracking tracking = new Tracking(OrderPriceCalculator: null, Postamats: null, Orders: null, Products: null, Lines: null, Customers: mock.Object);

            //Act
            Func<Order> act = () => tracking.CreateOrder(new OrderInputDto { PhoneNumber = phoneNumber });

            //Assert
            try
            {
                act();
            }
            catch (Exception e)
            {
                if (expectsException)
                {
                    Assert.True(e.GetType() == typeof(IncorrectPhoneNumber));
                }
                else
                {
                    Assert.True(e.GetType() != typeof(IncorrectPhoneNumber));
                }
            }
        }
        
        [Theory]
        [InlineData("111111", typeof(IncorrectPostamatNumber))]
        [InlineData("1111-112", typeof(PostamatClosed))]
        [InlineData("1111-113", typeof(PostamatNotFound))]
        [InlineData("1111-111", null)]
        public void CanThrowPostamatValidationExceptionWhenOrderCreated(string postamatNumber, Type exceptionType)
        {
            // Arrange
            var customersMock = new Mock<IRepository<Customer>>();
            customersMock.Setup(m => m.Get(It.IsAny<int>())).Returns((int id) => new Customer());

            var postamatsMock = new Mock<IRepository<Models.Postamat>>();
            postamatsMock.Setup(m => m.GetAll()).Returns((new Models.Postamat[] 
            { 
                new Models.Postamat { Number = "1111-111", IsWorking = true },
                new Models.Postamat { Number = "1111-112", IsWorking = false }
            }).AsQueryable());

            ITracking tracking = new Tracking(OrderPriceCalculator: null, Postamats: postamatsMock.Object, Orders: null, Products: null, Lines: null, Customers: customersMock.Object);

            //Act
            Action act = () => tracking.CreateOrder(new OrderInputDto { PostamatNumber = postamatNumber, PhoneNumber = "+7999-999-99-99" });

            //Assert
            try
            {
                act();
            }
            catch (Exception e)
            {
                if (exceptionType != null)
                {
                    Assert.True(e.GetType() == exceptionType);
                }
                else
                {
                    Assert.True(e.GetType() != exceptionType);
                }
            }
        }

        [Fact]
        public void CanThrowProductsCountExceededExceptionWhenOrderCreated()
        {
            // Arrange
            var customersMock = new Mock<IRepository<Customer>>();
            customersMock.Setup(m => m.Get(It.IsAny<int>())).Returns((int id) => new Customer());

            var postamatsMock = new Mock<IRepository<Models.Postamat>>();
            postamatsMock.Setup(m => m.GetAll()).Returns((new Models.Postamat[]
            {
                new Models.Postamat { Number = "1111-111", IsWorking = true }
            }).AsQueryable());

            var productsMock = new Mock<IRepository<Product>>();
            productsMock.Setup(m => m.GetAll()).Returns(Enumerable.Range(1, 12).Select(i => new Product { Name = $"product #{i}" }).AsQueryable());

            var products = Enumerable.Range(1, 11).Select(i => $"product #{i}").ToList();

            ITracking tracking = new Tracking(OrderPriceCalculator: null, Postamats: postamatsMock.Object, Orders: null, Products: productsMock.Object, Lines: null, Customers: customersMock.Object);

            //Act
            Action act = () => tracking.CreateOrder(new OrderInputDto { PhoneNumber = "+7999-999-99-99", PostamatNumber = "1111-111", Products = products });

            //Assert
            Assert.Throws<ProductsCountExceeded>(() => act());
        }
        
        [Fact]
        public void CanThrowProductsNotFoundExceptionWhenOrderCreated()
        {
            // Arrange
            var customersMock = new Mock<IRepository<Customer>>();
            customersMock.Setup(m => m.Get(It.IsAny<int>())).Returns((int id) => new Customer());

            var postamatsMock = new Mock<IRepository<Models.Postamat>>();
            postamatsMock.Setup(m => m.GetAll()).Returns((new Models.Postamat[]
            {
                new Models.Postamat { Number = "1111-111", IsWorking = true }
            }).AsQueryable());

            var productsMock = new Mock<IRepository<Product>>();
            productsMock.Setup(m => m.GetAll()).Returns(Enumerable.Range(1, 5).Select(i => new Product { Name = $"product #{i}" }).AsQueryable());

            var products = Enumerable.Range(6, 7).Select(i => $"product #{i}").ToList();

            ITracking tracking = new Tracking(OrderPriceCalculator: null, Postamats: postamatsMock.Object, Orders: null, Products: productsMock.Object, Lines: null, Customers: customersMock.Object);

            //Act
            Action act = () => tracking.CreateOrder(new OrderInputDto { PhoneNumber = "+7999-999-99-99", PostamatNumber = "1111-111", Products = products });

            //Assert
            Assert.Throws<ProductNotFound>(() => act());
        }

        [Fact]
        public void CanCreateOrder()
        {
            // Arrange
            var ordersMock = new Mock<IRepository<Order>>();
            var orders = new List<Order>();
            ordersMock.Setup(m => m.Create(It.IsAny<Order>())).Returns((Order order) =>
            {
                orders.Add(order);
                return order;
            });

            var customersMock = new Mock<IRepository<Customer>>();
            var customers = new Customer[]
            {
                new Customer { ID = 1 },
                new Customer { ID = 2 }
            };
            customersMock.Setup(m => m.Get(It.IsAny<int>())).Returns((int id) => customers.FirstOrDefault(c => c.ID == id));

            var postamatsMock = new Mock<IRepository<Models.Postamat>>();
            var postamats = new Models.Postamat[]
            {
                new Models.Postamat { Number = "1111-111", IsWorking = true },
                new Models.Postamat { Number = "1111-112", IsWorking = true }
            };
            postamatsMock.Setup(m => m.GetAll()).Returns(postamats.AsQueryable());

            var productsMock = new Mock<IRepository<Product>>();
            var products = Enumerable.Range(1, 5).Select(i => new Product { Name = $"product #{i}", Price = i }).ToList();
            productsMock.Setup(m => m.GetAll()).Returns(products.AsQueryable());


            var orderInfo = new OrderInputDto
            {
                CustomerID = 2,
                PostamatNumber = "1111-112",
                CustomerName = "Alex",
                PhoneNumber = "+7999-999-99-99",
                Products = new List<string> { "product #1", "product #2", "product #2" }
            };

            ITracking tracking = new Tracking(
                OrderPriceCalculator: new OrderPriceCalculator(),
                Postamats: postamatsMock.Object,
                Orders: ordersMock.Object,
                Products: productsMock.Object,
                Lines: null,
                Customers: customersMock.Object);

            // Act
            tracking.CreateOrder(orderInfo);
            
            // Assert
            Assert.True(orders.Any());
            Order order = orders.First();
            Assert.True(order.Status == 1);
            Assert.True(order.Lines.Count == 2);
            Assert.Equal(order.Lines[0].Product, products[0]);
            Assert.True(order.Lines[0].Quantity == 1);
            Assert.Equal(order.Lines[1].Product, products[1]);
            Assert.True(order.Lines[1].Quantity == 2);
            Assert.True(order.Price == 5);
            Assert.Equal(order.Postamat, postamats[1]);
            Assert.True(order.Name == "Alex");
            Assert.True(order.PhoneNumber == "+7999-999-99-99");
            Assert.Equal(order.Customer, customers[1]);
        }

        [Fact]
        public void CanUpdateOrder()
        {
            // Arrange
            var productsMock = new Mock<IRepository<Product>>();
            var products = Enumerable.Range(1, 5).Select(i => new Product { Name = $"product #{i}", Price = i }).ToList();
            productsMock.Setup(m => m.GetAll()).Returns(products.AsQueryable());

            var linesMock = new Mock<IRepository<CartLine>>();
            var lines = new List<CartLine>
            {
                new CartLine { ID = 1, Product = products[0], Quantity = 1 },
                new CartLine { ID = 2, Product = products[1], Quantity = 2 }
            };
            linesMock
                .Setup(m => m.GetAll(It.IsAny<Func<IQueryable<CartLine>, IIncludableQueryable<CartLine, object>>>()))
                .Returns((Func<IQueryable<CartLine>, IIncludableQueryable<CartLine, object>> include) => include(lines.AsQueryable()));
            linesMock
                .Setup(m => m.Delete(It.IsAny<int>()))
                .Returns((int id) =>
                {
                    var toRemove = lines.FirstOrDefault(l => l.ID == id);
                    lines.Remove(toRemove);
                    return toRemove;
                });

            var ordersMock = new Mock<IRepository<Order>>();
            var orders = new List<Order>
            {
                new Order { ID = 1, Price = 5, PhoneNumber = "+7999-999-99-99", Name = "Alex",  Lines = new List<CartLine> { lines[0], lines[1] } },
            };
            lines[0].Order = orders[0];
            lines[1].Order = orders[0];
            ordersMock
                .Setup(m => m.Get(It.IsAny<int>(), It.IsAny<Func<IQueryable<Order>, IIncludableQueryable<Order, object>>>()))
                .Returns((int id, Func<IQueryable<Order>, IIncludableQueryable<Order, object>> include) => orders.FirstOrDefault(orders => orders.ID == id));
            ordersMock.Setup(m => m.Update(It.IsAny<Order>())).Returns((Order order) =>
            {
                order.Lines.ForEach(cartLine =>
                {
                    cartLine.Order = order;
                    lines.Add(cartLine);
                });
                int index = orders.IndexOf(orders.FirstOrDefault(o => o.ID == order.ID));
                orders[index] = order;
                return order;
            });


            var orderInfo = new OrderInputDto
            {
                ID = 1,
                PhoneNumber = "+7999-999-99-90",
                CustomerName = "Max",
                Products = new List<string> { "product #3", "product #4", "product #4", "product #5" }
            };

            ITracking tracking = new Tracking(
                OrderPriceCalculator: new OrderPriceCalculator(),
                Postamats: null,
                Orders: ordersMock.Object,
                Products: productsMock.Object,
                Lines: linesMock.Object,
                Customers: null);

            // Act
            var updatetOrder = tracking.UpdateOrder(orderInfo);

            // Assert
            Order order = orders.FirstOrDefault(o => o.ID == orderInfo.ID);
            Assert.True(order.Price == 16);
            Assert.True(order.Name == "Max");
            Assert.True(order.PhoneNumber == "+7999-999-99-90");
            var orderLines = lines.Where(l => l.Order.ID == orderInfo.ID).ToList();
            Assert.True(orderLines.Count() == 3);
            Assert.Equal(orderLines[0].Product, products[2]);
            Assert.True(orderLines[0].Quantity == 1);
            Assert.Equal(orderLines[1].Product, products[3]);
            Assert.True(orderLines[1].Quantity == 2);
            Assert.Equal(orderLines[2].Product, products[4]);
            Assert.True(orderLines[2].Quantity == 1);
            orderLines.ForEach(line => Assert.Equal(line.Order, updatetOrder));
        }
    }
}
