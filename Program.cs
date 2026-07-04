using Azure.Messaging.ServiceBus;
using Azure.Identity;
using System.Timers;
using Configuration;


string svcbusNameSpace = AppConfiguration.ServiceBusNamespaceUrl;
string queueName = AppConfiguration.ServiceBusQueueName;

DefaultAzureCredentialOptions options = new()
{
    ExcludeEnvironmentCredential = true,
    ExcludeManagedIdentityCredential = true
};

ServiceBusClient client = new(svcbusNameSpace, new DefaultAzureCredential(options));

ServiceBusSender sender = client.CreateSender(queueName);

using ServiceBusMessageBatch messageBatch = await sender.CreateMessageBatchAsync();
const int numOfMessages = 3;

for (int i = 1; i <= numOfMessages; i++)
{
    if (!messageBatch.TryAddMessage(new ServiceBusMessage($"Message {i}")))
    {
        throw new Exception($"The message {i} is too large to fit in the batch.");
    }
}

try
{
    await sender.SendMessagesAsync(messageBatch);
    Console.WriteLine($"A batch of {numOfMessages} messages has been published to the queue.");
}
finally
{
    await sender.DisposeAsync();
}

Console.WriteLine("Press any key to continue");
Console.ReadKey();

ServiceBusProcessor processor = client.CreateProcessor(queueName, new ServiceBusProcessorOptions());

const int idleTimeoutMs = 3000;
System.Timers.Timer idleTimer = new(idleTimeoutMs);
idleTimer.Elapsed += async (s, e) =>
{
    Console.WriteLine($"No messages received for {idleTimeoutMs / 1000} seconds. Stopping processor...");
    await processor.StopProcessingAsync();
};

try
{
    processor.ProcessMessageAsync += MessageHandler;

    processor.ProcessErrorAsync += ErrorHandler;

    idleTimer.Start();
    await processor.StartProcessingAsync();

    Console.WriteLine($"Processor started. Will stop after {idleTimeoutMs / 1000} seconds of inactivity.");
    while (processor.IsProcessing)
    {
        await Task.Delay(500);
    }
    idleTimer.Stop();
    Console.WriteLine("Stopped receiving messages");
}
finally
{
    await processor.DisposeAsync();
}

async Task MessageHandler(ProcessMessageEventArgs args)
{
    string body = args.Message.Body.ToString();
    Console.WriteLine($"Received: {body}");

    // Reset the idle timer on each message
    idleTimer.Stop();
    idleTimer.Start();

    await args.CompleteMessageAsync(args.Message);
}
Task ErrorHandler(ProcessErrorEventArgs args)
{
    Console.WriteLine(args.Exception.ToString());
    return Task.CompletedTask;
}

await client.DisposeAsync();