namespace Postamat.Models
{
    public class CartLine : DbEntity
    {
        public Product Product { get; set; }
        public int Quantity { get; set; }

        public Order Order { get; set; }
    }
}