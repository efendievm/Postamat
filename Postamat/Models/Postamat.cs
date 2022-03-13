using System.Collections.Generic;

namespace Postamat.Models
{
    public class Postamat : DbEntity
    {
        public string Number { get; set; }
        public string Address { get; set; }
        public bool IsWorking { get; set; }

        public List<Order> Orders { get; set; }
    }
}
