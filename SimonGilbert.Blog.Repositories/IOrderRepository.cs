using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Table;
using SimonGilbert.Blog.Models;

namespace SimonGilbert.Blog.Repositories
{
    public interface IOrderRepository
    {
        Task<TableResult> Create(Order model);

        Task<TableResult> Update(string partitionKey, string rowKey, OrderStatus orderStatus);

        Task<Order> Get(string partitionKey, string rowKey);
    }
}
