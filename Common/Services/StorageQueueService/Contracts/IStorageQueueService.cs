using System.Threading.Tasks;

namespace Common.Services.StorageQueueService.Contracts
{
    public interface IStorageQueueService
    {
        Task AddNewMessage(string messageBody, string queueName);

        Task ReceiveMessagesAsync(string connectionString, string queueName);
    }
}
