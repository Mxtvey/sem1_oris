namespace MiniHttpServer.Core.Attributes
{
    [AttributeUsage(AttributeTargets.Method)]
    public class HttpPOST : Attribute
    {
        public string? Route { get; }

        public HttpPOST()
        {
        }

        public HttpPOST(string? route)
        {
            Route = route;
        }
    }
}