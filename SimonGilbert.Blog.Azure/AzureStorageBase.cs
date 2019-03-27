using Microsoft.WindowsAzure.Storage.Table;

namespace SimonGilbert.Blog.Azure
{
    public abstract class AzureStorageBase
    {
        protected string GenerateCombinedKeyFilter(string partitionKey, string rowKey)
        {
            return TableQuery.CombineFilters(
                GeneratePartitionKeyFilter(partitionKey),
                TableOperators.And,
                GenerateRowKeyFilter(rowKey));
        }

        protected string GeneratePartitionKeyFilter(string partitionKey)
        {
            return TableQuery.GenerateFilterCondition(
                "PartitionKey", QueryComparisons.Equal, partitionKey);
        }

        protected string GenerateRowKeyFilter(string rowKey)
        {
            return TableQuery.GenerateFilterCondition(
                "RowKey", QueryComparisons.Equal, rowKey);
        }
    }
}