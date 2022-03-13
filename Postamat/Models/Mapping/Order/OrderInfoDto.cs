using System.Collections.Generic;

namespace Postamat.Models.Mapping
{
    /// <summary>
    /// DTO заказа для предоставления информации пользователю.
    /// </summary>
    public class OrderInfoDto
    {
        public int ID { get; set; }
        public string Status { get; set; }
        public List<string> Products { get; set; }
        public decimal Price { get; set; }
        public string PostamatNumber { get; set; }
        public string CustomerName { get; set; }
        public string PhoneNumber { get; set; }
    }
}
