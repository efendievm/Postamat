using System;

namespace Postamat.Exceptions
{
    public class ProductsCountExceeded : Exception
    {
        public int Count { get; private set; }
        public ProductsCountExceeded(int Count) : base("Products count should not exceed 10") => this.Count = Count;
    }
}
