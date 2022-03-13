using Postamat.DataBases;
using Postamat.Models;

namespace Postamat.Repositories
{
    /// <summary>
    /// Репозиторий зарегестированных пользователей.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class IdentityRepository<T> : Repository<T> where T : DbEntity
    {
        public IdentityRepository(IdentityContext context) : base(context) { }
    }
}