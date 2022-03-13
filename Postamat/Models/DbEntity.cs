namespace Postamat.Models
{
    /// <summary>
    /// Родительский класс для всех сущностей из БД, предоставляющий ID для сущности.
    /// </summary>
    public class DbEntity
    {
        public int ID { get; set; }
    }
}
