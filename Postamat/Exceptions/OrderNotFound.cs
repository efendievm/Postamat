using System;

namespace Postamat.Exceptions
{
    public class OrderNotFound : Exception
    {
        public int ID { get; private set; }
        public OrderNotFound(int ID) : base($"Order {ID} not found") => this.ID = ID;
    }
}
