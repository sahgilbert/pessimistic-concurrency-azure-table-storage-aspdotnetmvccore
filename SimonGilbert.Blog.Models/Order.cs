using Microsoft.WindowsAzure.Storage.Table;

namespace SimonGilbert.Blog.Models
{
    public class Order : TableEntity
    {
        public string Status { get; set; }
    }
}
