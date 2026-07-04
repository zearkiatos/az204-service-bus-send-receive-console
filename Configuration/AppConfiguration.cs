using dotenv.net;

namespace Configuration
{
    public static class AppConfiguration
    {
        private static IDictionary<string, string> envVars = null;
        static AppConfiguration()
        {
            DotEnv.Load();
            envVars = DotEnv.Read();
        }

        public static string ServiceBusNamespaceUrl => 
            envVars["SERVICE_BUS_NAMESPACE_URL"] 
            ?? throw new InvalidOperationException("SERVICE_BUS_NAMESPACE_URL environment variable is not set");

        public static string ServiceBusQueueName => 
            envVars["SERVICE_BUS_QUEUE_NAME"] 
            ?? throw new InvalidOperationException("SERVICE_BUS_QUEUE_NAME environment variable is not set");

        public static void ValidateConfiguration()
        {
            try
            {
                _ = ServiceBusNamespaceUrl;
                _ = ServiceBusQueueName;
                Console.WriteLine("✓ Configuration validation passed");
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine($"✗ Configuration validation failed: {ex.Message}");
                throw;
            }
        }
    }
}