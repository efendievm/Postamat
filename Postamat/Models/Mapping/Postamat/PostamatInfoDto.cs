namespace Postamat.Models.Mapping
{
    /// <summary>
    /// DTO постамата для предоставления информации пользователю.
    /// </summary>
    public class PostamatInfoDto
    {
        public string Number { get; set; }
        public string Address { get; set; }
        public string IsWorking { get; set; }
    }
}