using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Table;
using SimonGilbert.Blog.Models;

namespace SimonGilbert.Blog.Services
{
    public interface IOrderService
    {
        Task<TableResult> Create(string accountId, string orderId);

        Task Update(string accountId, string orderId, OrderStatus orderStatus);

        Task<Order> Get(string accountId, string orderId);
    }
}
