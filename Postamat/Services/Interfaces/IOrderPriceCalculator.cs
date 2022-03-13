using Postamat.Models;
using System.Collections.Generic;

namespace Postamat.Services
{
    public interface IOrderPriceCalculator
    {
        decimal GetPrice(List<CartLine> Lines);
    }
}