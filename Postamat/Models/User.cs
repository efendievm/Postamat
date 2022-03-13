namespace Postamat.Models
{
    public class User : DbEntity
    {
        public string Login { get; set; }
        public string Password { get; set; }
        public string Role { get; set; }
        // Поле, по которому устанавливается связь между объектами User из БД IdentityContext и Customer из БД ApplicationContext
        public int CustomerID { get; set; }
    }
}