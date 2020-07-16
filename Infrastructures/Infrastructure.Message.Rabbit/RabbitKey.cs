namespace Infrastructure.Message.Rabbit
{
    public class RabbitKey
    {
        public static class Exchanges
        {
            public static string Events = "events";
            public static string Commands = "commands";
            public static string Queries = "queries";
            public static string Rpc = "rpc";
        }
    }
}
