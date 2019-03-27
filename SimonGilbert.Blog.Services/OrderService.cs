using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Table;
using SimonGilbert.Blog.Models;
using SimonGilbert.Blog.Repositories;

namespace SimonGilbert.Blog.Services
{
    public class OrderService : IOrderService
    {
        private readonly IConfiguration _configuration;
        private readonly IOrderRepository _repository;
        private readonly CloudBlobContainer _cloudBlobContainer;
        private readonly TimeSpan _minimumAcquireLeaseTimeSpan = TimeSpan.FromSeconds(15);

        public OrderService(
            IConfiguration configuration,
            IOrderRepository repository,
            CloudBlobContainer cloudBlobContainer)
        {
            this._configuration = configuration;
            this._repository = repository;
            this._cloudBlobContainer = cloudBlobContainer;
        }

        public async Task<TableResult> Create(string accountId, string orderId)
        {
            await CreateCloudBlockBlobForLease(orderId);

            var model = new Order
            {
                PartitionKey = accountId,
                RowKey = orderId,
                Status = OrderStatus.Created.ToString(),
            };

            var result = await _repository.Create(model);

            return result;
        }

        public async Task Update(string accountId, string orderId, OrderStatus orderStatus)
        {
            var cloudBlockBlob = GetCloudBlockBlob(orderId);

            var leaseId = await cloudBlockBlob.AcquireLeaseAsync(
                _minimumAcquireLeaseTimeSpan, 
                Guid.NewGuid().ToString());

            try
            {
                await _repository.Update(accountId, orderId, orderStatus);
            }
            finally
            {
                await cloudBlockBlob.ReleaseLeaseAsync(AccessCondition.GenerateLeaseCondition(leaseId));
            }
        }

        public async Task<Order> Get(string accountId, string orderId)
        {
            var result = await _repository.Get(accountId, orderId);

            return result;
        }

        private async Task CreateCloudBlockBlobForLease(string blobName)
        {
            var cloudBlockBlob = GetCloudBlockBlob(blobName);

            await cloudBlockBlob.UploadTextAsync("");
        }

        private CloudBlockBlob GetCloudBlockBlob(string blobName)
        {
            var cloudLockBlobLeaseName = GenerateCloudLockBlobLeaseName(blobName);

            var cloudBlockBlob = _cloudBlobContainer.GetBlockBlobReference(cloudLockBlobLeaseName);

            return cloudBlockBlob;
        }

        private string GenerateCloudLockBlobLeaseName(string blobLeaseNamePrefix)
        {
            return $"{blobLeaseNamePrefix}.lck";
        }
    }
}
