using System.Collections.Generic;
namespace Postamat.Models.Mapping
{
    /// <summary>
    /// DTO заказа для изменения или создания заказа.
    /// </summary>
    public class OrderInputDto
    {
        public int ID { get; set; }
        public string PostamatNumber { get; set; }
        public string CustomerName { get; set; }
        public string PhoneNumber { get; set; }
        public List<string> Products { get; set; } = new List<string>();
        public int CustomerID { get; set; }
    }
}
