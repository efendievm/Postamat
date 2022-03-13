namespace Postamat.Models.Mapping
{
    /// <summary>
    /// DTO заказа для предоставления информации админу.
    /// </summary>
    public class OrderAdminInfoDto
    {
        public int ID { get; set; }
        public string Status { get; set; }
        public decimal Price { get; set; }
        public string PostamatNumber { get; set; }
        public string CustomerName { get; set; }
        public string PhoneNumber { get; set; }
        public int CustomerID { get; set; }
    }
}
