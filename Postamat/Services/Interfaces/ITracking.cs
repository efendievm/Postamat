using Postamat.Models;
using Postamat.Models.Mapping;

namespace Postamat.Services
{
    public interface ITracking
    {
        Order CreateOrder(OrderInputDto order);
        Order UpdateOrder(OrderInputDto order);
        Order UpdateOrder(int id, string postamatNumber, int status);
        Order CancelOrder(int id);
    }
}
