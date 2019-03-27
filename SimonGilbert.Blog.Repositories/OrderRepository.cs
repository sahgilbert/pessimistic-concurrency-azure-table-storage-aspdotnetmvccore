using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Table;
using SimonGilbert.Blog.Azure;
using SimonGilbert.Blog.Models;

namespace SimonGilbert.Blog.Repositories
{
    public class OrderRepository : AzureStorageBase, IOrderRepository
    {
        private readonly CloudTable _table;

        public OrderRepository(CloudTable table)
        {
            this._table = table;
        }

        public async Task<TableResult> Create(Order model)
        {
            var result = await _table.ExecuteAsync(TableOperation.Insert(model));

            return result;
        }

        public async Task<TableResult> Update(string partitionKey, string rowKey, OrderStatus orderStatus)
        {
            var model = await Get(partitionKey, rowKey);

            model.Status = orderStatus.ToString();

            var result = await _table.ExecuteAsync(TableOperation.Replace(model));

            return result;
        }

        public async Task<Order> Get(string partitionKey, string rowKey)
        {
            var result = await _table.ExecuteAsync(
                TableOperation.Retrieve<Order>(partitionKey, rowKey));

            return (Order)result.Result;
        }
    }
}
