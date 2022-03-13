using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Postamat.Models;
using Postamat.Services;
using System.Collections.Generic;
using System.Linq;

namespace Postamat.DataBases
{
    public static class ContextSeedData
    {
        /// <summary>
        /// Заполнение БД начальными данными.
        /// </summary>
        /// <param name="app"></param>
        public static void EnsurePopulated(IApplicationBuilder app)
        {
            ApplicationContext appContext = app.ApplicationServices.GetRequiredService<ApplicationContext>();
            appContext.Database.Migrate();
            if (!appContext.Products.Any())
            {
                appContext.Products.AddRange(
                    new Product { Name = "Lifejacket", Price = 48.95m },
                    new Product { Name = "Soccer Ball", Price = 19.50m },
                    new Product { Name = "Corner Flags", Price = 34.95m },
                    new Product { Name = "Thinking Сар", Price = 16 },
                    new Product { Name = "Chess Board", Price = 75 });
                appContext.SaveChanges();
            }

            if (!appContext.Postamats.Any())
            {
                appContext.Postamats.AddRange(
                    new Models.Postamat { Number = "1234-567", Address = "County Rd #7 15 Shortsville", IsWorking = true },
                    new Models.Postamat { Number = "7654-321", Address = "Jewett Holmwood Rdu Orchard Park", IsWorking = true },
                    new Models.Postamat { Number = "5678-234", Address = "Sutphin Blvd #B311r Jamaica", IsWorking = true },
                    new Models.Postamat { Number = "8897-874", Address = "Farley Rd Hudson Falls", IsWorking = true },
                    new Models.Postamat { Number = "9634-781", Address = "Scott Rd Frewsburg", IsWorking = true });
                appContext.SaveChanges();
            }

            if (!appContext.Customers.Any())
            {
                appContext.Customers.AddRange(new Customer(), new Customer());
                appContext.SaveChanges();
            }

            if (!appContext.CartLines.Any())
            {
                var products = appContext.Products.ToList();
                appContext.CartLines.AddRange(
                    new CartLine { Product = products[0], Quantity = 1 },
                    new CartLine { Product = products[1], Quantity = 2 },
                    new CartLine { Product = products[2], Quantity = 1 },
                    new CartLine { Product = products[3], Quantity = 1 },
                    new CartLine { Product = products[4], Quantity = 1 });
            }

            appContext.SaveChanges();

            if (!appContext.Orders.Any())
            {
                IOrderPriceCalculator priceCalculator = app.ApplicationServices.GetRequiredService<IOrderPriceCalculator>();
                appContext.CartLines.RemoveRange(appContext.CartLines);

                var products = appContext.Products.ToList();
                var postamats = appContext.Postamats.ToList();
                var customers = appContext.Customers.ToList();
                var productSets = new List<List<Product>>() 
                { 
                    new List<Product>() { products[0], products[1], products[1] },
                    new List<Product>() { products[2], products[3], products[4] },
                    new List<Product>() { products[1], products[2], products[3] },
                    new List<Product>() { products[0], products[2], products[4] }
                };
                var customersData = new List<(string Name, string PhoneNumber)>
                {
                    ("alexander", "+7985-983-09-23"),
                    ("lisa", "+7934-892-76-30"),
                    ("helga", "+7872-543-03-58"),
                    ("mark", "+7563-124-94-27"),
                };
                Order CreateOrder(int productSetID, int postamatID, int customerID, int customerDataID)
                {
                    var Lines = productSets[productSetID]
                        .GroupBy(p => p.Name)
                        .Select(line => new CartLine { Product = products.FirstOrDefault(p => p.Name == line.Key), Quantity = line.Count() })
                        .ToList();
                    return new Order
                    {
                        Status = 1,
                        Lines = Lines,
                        Postamat = postamats[postamatID],
                        Customer = customers[customerID],
                        Name = customersData[customerDataID].Name,
                        PhoneNumber = customersData[customerDataID].PhoneNumber,
                        Price = priceCalculator.GetPrice(Lines),
                    };
                }
                
                
                appContext.Orders.AddRange(
                    CreateOrder(productSetID: 0, postamatID: 0, customerID: 0, customerDataID: 0),
                    CreateOrder(productSetID: 1, postamatID: 1, customerID: 0, customerDataID: 1),
                    CreateOrder(productSetID: 2, postamatID: 2, customerID: 1, customerDataID: 2),
                    CreateOrder(productSetID: 3, postamatID: 3, customerID: 1, customerDataID: 3));

                appContext.SaveChanges();
            }


            IdentityContext identityContext = app.ApplicationServices.GetRequiredService<IdentityContext>();
            identityContext.Database.Migrate();
            if (!identityContext.Users.Any())
                identityContext.Users.AddRange(
                    new User { Login = "admin", Password = "admin", Role = "admin" },
                    new User { Login = "alex", Password = "alex", Role = "customer", CustomerID = appContext.Customers.ToList()[0].ID },
                    new User { Login = "max", Password = "max", Role = "customer", CustomerID = appContext.Customers.ToList()[1].ID });
            
            identityContext.SaveChanges();
        }
    }
}