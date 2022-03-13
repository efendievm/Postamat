using Postamat.DataBases;
using Postamat.Models;

namespace Postamat.Repositories
{
    /// <summary>
    /// Обобщенный репозиторий приложения.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ApplicationRepository<T> : Repository<T> where T : DbEntity
    {
        public ApplicationRepository(ApplicationContext context) : base(context) { }
    }
}