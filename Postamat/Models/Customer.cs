using System.Collections.Generic;

namespace Postamat.Models
{
    public class Customer : DbEntity
    {
        public List<Order> Orders { get; set; }
    }
}
