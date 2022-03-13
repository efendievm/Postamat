using System.Collections.Generic;

namespace Postamat.Models
{
    public class Product : DbEntity
    {
        public string Name { get; set; }
        public decimal Price { get; set; }

        public List<CartLine> Lines { get; set; }
    }
}
