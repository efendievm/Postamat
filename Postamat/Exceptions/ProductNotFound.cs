using System;

namespace Postamat.Exceptions
{
    public class ProductNotFound : Exception
    {
        public string Name { get; private set; }
        public ProductNotFound(string Name) : base($"Product {Name} not found") => this.Name = Name;
    }
}