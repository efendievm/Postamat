namespace Postamat.Models.Mapping
{
    /// <summary>
    /// DTO товара для предоставления информации пользователю.
    /// </summary>
    public class ProductInfoDto
    {
        public string Name { get; set; }
        public decimal Price { get; set; }
    }
}