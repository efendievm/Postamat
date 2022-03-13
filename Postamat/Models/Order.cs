using System.Collections.Generic;

namespace Postamat.Models
{
    public class Order : DbEntity
    {
        public int Status { get; set; }
        public List<CartLine> Lines { get; set; }
        public decimal Price { get; set; }
        public Postamat Postamat { get; set; }
        public string Name { get; set; }
        public string PhoneNumber { get; set; }
        public Customer Customer { get; set; }
    }
}