namespace MiniHttpServer.Framework.Core.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class EndpointAttribute : Attribute
    {
        public string? Route { get; }

        public EndpointAttribute() { }

        public EndpointAttribute(string route)
        {
            Route = route;
        }
    }
}