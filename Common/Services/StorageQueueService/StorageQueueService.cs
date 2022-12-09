using System;
using System.Text;
using System.Threading.Tasks;
using Azure.Identity;
using Azure.Storage.Queues;
using Common.Services.StorageQueueService.Contracts;

namespace Common.Services.StorageQueueService;

public class StorageQueueService : IStorageQueueService
{
    private readonly string _connectionString;

    public StorageQueueService(string connectionString)
    {
        _connectionString = connectionString;
    }
    
    public async Task AddNewMessage(string messageBody, string queueName)
    {
        var queueClient = GetQueueClient(queueName);
        var validMessage = !IsBase64Encoded(messageBody) ? Base64Encode(messageBody) : messageBody;
        
        await queueClient.SendMessageAsync(validMessage);
    }

    public async Task ReceiveMessagesAsync(string connectionString, string queueName)
    {
        var queueClient = GetQueueClient(queueName);
        foreach (var message in (await queueClient.ReceiveMessagesAsync(maxMessages: 5)).Value)
        {
            // "Process" the message
            Console.WriteLine($"Message: {message.Body}");

            // Let the service know we're finished with the message and it can be safely deleted.
            await queueClient.DeleteMessageAsync(message.MessageId, message.PopReceipt);
        }
    }

    private QueueClient GetQueueClient(string queueName)
    {
        var credential = new DefaultAzureCredential();
        
        
        
        
        return new QueueClient(new Uri(string.Format(_connectionString, queueName)), credential);
    }
    
    private static bool IsBase64Encoded(string str)
    {
        try
        {
            // If no exception is caught, then it is possibly a base64 encoded string
            _ = Convert.FromBase64String(str);
            // perform a final check to make sure that the string was properly padded to the correct length
            return str.Replace(" ","").Length % 4 == 0;
        }
        catch
        {
            // If exception is caught, then it is not a base64 encoded string
            return false;
        }
    }
    
    private static string Base64Encode(string plainText)
    {
        var plainTextBytes = Encoding.UTF8.GetBytes(plainText);
        return Convert.ToBase64String(plainTextBytes);
    }
}
