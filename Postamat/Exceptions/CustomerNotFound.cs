using System;

namespace Postamat.Exceptions
{
    public class CustomerNotFound : Exception
    {
        public int ID { get; private set; }
        public CustomerNotFound(int ID) : base($"Customer {ID} not found") => this.ID = ID;
    }
}
