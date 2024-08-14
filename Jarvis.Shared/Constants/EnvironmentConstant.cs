namespace Jarvis.Shared.Constants
{
    public static class EnvironmentConstant
    {
        public static class Database
        {
            public static string ConnectionStringPrefix = "CONNECTION_STRINGS__";
        }

        public static class EventBridge
        {
            public static string AccessKey = "DISTRIBUTED_EVENT__EVENT_BRIDGE__ACCESSKEY";
            public static string SecretKey = "DISTRIBUTED_EVENT__EVENT_BRIDGE__SECRETKEY";
        }

        public static class Cognito
        {
            public static string AccessKey = "AUTHENTICATION__COGNITO__ACCESSKEY";
            public static string SecretKey = "AUTHENTICATION__COGNITO__SECRETKEY";
        }

        public static class MarketplaceService
        {
            public static string ClientId = "SERVICES_MARKETPLACE__CLIENT_ID";
            public static string ClientSecret = "SERVICES_MARKETPLACE__CLIENT_SECRET";
        }
    }
}