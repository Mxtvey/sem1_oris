using System;

namespace MiniHttpServer.Framework.Core.Attributes
{
    [AttributeUsage(AttributeTargets.Method)]
    public class HttpGETAttribute : Attribute
    {
        public string? Route { get; }

        public HttpGETAttribute() {}

        public HttpGETAttribute(string route)
        {
            Route = route;
        }
    }
}