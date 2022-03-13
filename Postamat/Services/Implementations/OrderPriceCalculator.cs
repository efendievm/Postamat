using Postamat.Models;
using System.Collections.Generic;
using System.Linq;

namespace Postamat.Services
{
    public class OrderPriceCalculator : IOrderPriceCalculator
    {
        public decimal GetPrice(List<CartLine> Lines)
        {
            decimal price = Lines.Sum(l => l.Product.Price * l.Quantity);
            return price > 10000 ? (decimal)0.95 * price : price;
        }
    }
}