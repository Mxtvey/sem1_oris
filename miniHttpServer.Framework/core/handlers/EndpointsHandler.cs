using System.Net;
using System.Reflection;
using System.Text;
using MiniHttpServer.Framework.Core.Abstracts;
using MiniHttpServer.Framework.Core.Attributes;
using MiniHttpServer.HttpResponce;

namespace HttpServer.Framework.core.handlers
{
    internal class EndpointsHandler : Handler
    {
        public override void HandleRequest(HttpListenerContext context)
        {
            try
            {   
                var query = context.Request.Url.Query; 

                string path = Normalize(context.Request.Url?.AbsolutePath);  
                            

                string methodName = $"Http{context.Request.HttpMethod}";

                

          
                var endpointTypes = new List<Type>();
                foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    try
                    {
                        if (assembly.IsDynamic) continue;

                        var types = assembly.GetTypes();
                        foreach (var type in types)
                        {
                            if (type.GetCustomAttribute<EndpointAttribute>() != null)
                            {
                                endpointTypes.Add(type);
                                
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(
                            $"[Error loading types from {assembly.GetName().Name}: {ex.Message}");
                    }
                }

              

                foreach (var endpointType in endpointTypes)
                {
                    var endpointAttr = endpointType.GetCustomAttribute<EndpointAttribute>();
                    string endpointBase = "/" + (endpointAttr?.Route?.Trim('/') ?? "");

                  

                  
                    

                    foreach (var method in endpointType.GetMethods())
                    {
                        var httpAttr = method.GetCustomAttributes()
                            .FirstOrDefault(a =>
                                a.GetType().Name.StartsWith(methodName, StringComparison.OrdinalIgnoreCase));


                        if (httpAttr == null)
                        {
                            continue;
                        }

                     

                       
                        var routeProperty = httpAttr.GetType().GetProperty("Route");
                        string? methodRoute = routeProperty?.GetValue(httpAttr) as string;

                      

                       
                        string fullRoute = BuildRoute(endpointBase, methodRoute);
                       

                    
                        if (path.Equals(fullRoute, StringComparison.OrdinalIgnoreCase))
                        {
                            
                            ExecuteEndpointMethod(context, endpointType, method);
                            return;
                        }
                       
                    }
                }
                
                Successor?.HandleRequest(context);
            }
            catch (Exception ex)
            {
               
                SafeWrite500(context);

            }
        }

        private static void ExecuteEndpointMethod(HttpListenerContext context, Type endpointType, MethodInfo method)
        {
            try
            {
                var instance = Activator.CreateInstance(endpointType);

              
                endpointType.GetMethod("SetContext",
                        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                    ?.Invoke(instance, new object[] { context });

                object? result;

                var ps = method.GetParameters();

             
                if (ps.Length == 0)
                {
                    result = method.Invoke(instance, null);
                }
                else
                {
                    var args = new object?[ps.Length];
                    var query = context.Request.QueryString;

                    for (int i = 0; i < ps.Length; i++)
                    {
                        var p = ps[i];

                       
                        if (p.ParameterType == typeof(HttpListenerContext))
                        {
                            args[i] = context;
                            continue;
                        }

                        string name = p.Name ?? string.Empty;
                        string? raw = query[name];

                      
                        if (string.IsNullOrWhiteSpace(raw))
                        {
                            
                            if (p.HasDefaultValue)
                            {
                                args[i] = p.DefaultValue;
                            }
                            else if (IsNullable(p.ParameterType))
                            {
                                args[i] = null;
                            }
                            else
                            {
                               
                                WriteBadRequest(context, $"Missing or empty required parameter '{name}'");
                                return;
                            }
                        }
                        else
                        {
                            try
                            {
                                args[i] = ConvertFromString(raw, p.ParameterType);
                            }
                            catch (Exception ex)
                            {
                                WriteBadRequest(context, $"Invalid format for parameter '{name}'");
                                return;
                            }
                        }
                    }

                    result = method.Invoke(instance, args);
                }

                if (result is Task task)
                {
                    task.GetAwaiter().GetResult();
                    var t = task.GetType();
                    result = t.IsGenericType ? t.GetProperty("Result")?.GetValue(task) : null;
                }

                if (result is IResponseResult rr)
                {
                    rr.Execute(context);
                    return;
                }

                if (result is string s)
                {
                    byte[] buffer = System.Text.Encoding.UTF8.GetBytes(s);
                    context.Response.ContentLength64 = buffer.Length;
                    context.Response.OutputStream.Write(buffer);
                    context.Response.OutputStream.Close();
                    return;
                }

                SafeWrite500(context);

            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Endpoint ERROR] {ex.Message}");
                SafeWrite500(context);
            }

        }

        private static string BuildRoute(string endpointBase, string? methodRoute)
        {
            if (string.IsNullOrWhiteSpace(methodRoute))
            {
                return endpointBase;
            }

            return endpointBase + "/" + methodRoute.Trim('/');
        }

        private static string Normalize(string? path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return "/";
            }

            string result = path;
            if (!result.StartsWith("/"))
            {
                result = "/" + result;
            }

            return result;
        }

        private static void SafeWrite500(HttpListenerContext ctx)
        {
            try
            {
                if (ctx.Response.OutputStream.CanWrite)
                {
                    ctx.Response.StatusCode = 500;
                    var bytes = Encoding.UTF8.GetBytes("Internal Server Error");
                    ctx.Response.OutputStream.Write(bytes, 0, bytes.Length);
                }
            }
            catch { }
            finally
            {
                try { ctx.Response.OutputStream.Close(); } catch {}
            }
        }


        private static void WriteBadRequest(HttpListenerContext ctx, string message)
        {
            try
            {
                if (!ctx.Response.OutputStream.CanWrite)
                {
                    Console.WriteLine("[WriteBadRequest] Response stream is closed");
                    return;
                }

                ctx.Response.StatusCode = 400;
                ctx.Response.ContentType = "text/plain; charset=utf-8";
                byte[] data = Encoding.UTF8.GetBytes(message);
                ctx.Response.ContentLength64 = data.Length;
                ctx.Response.OutputStream.Write(data, 0, data.Length);
                ctx.Response.OutputStream.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[WriteBadRequest] Failed to write error response: {ex.Message}");
            }
        }

        private static bool IsNullable(Type t)
        {
            if (!t.IsValueType) return true; 
            return Nullable.GetUnderlyingType(t) != null; 
        }

        private static object? ConvertFromString(string raw, Type targetType)
        {
            var underlying = Nullable.GetUnderlyingType(targetType) ?? targetType;

            if (underlying == typeof(string))
                return raw;

            if (underlying == typeof(int))
            {
                if (!int.TryParse(raw, out var vInt))
                    throw new FormatException($"Cannot convert '{raw}' to int");
                return vInt;
            }

            if (underlying == typeof(long))
            {
                if (!long.TryParse(raw, out var vLong))
                    throw new FormatException($"Cannot convert '{raw}' to long");
                return vLong;
            }

            if (underlying == typeof(bool))
            {
                if (!bool.TryParse(raw, out var vBool))
                    throw new FormatException($"Cannot convert '{raw}' to bool");
                return vBool;
            }

            if (underlying == typeof(decimal))
            {
                if (!decimal.TryParse(raw, out var vDec))
                    throw new FormatException($"Cannot convert '{raw}' to decimal");
                return vDec;
            }

            if (underlying == typeof(double))
            {
                if (!double.TryParse(raw, out var vDouble))
                    throw new FormatException($"Cannot convert '{raw}' to double");
                return vDouble;
            }

            if (underlying == typeof(DateTime))
            {
                if (!DateTime.TryParse(raw, out var vDate))
                    throw new FormatException($"Cannot convert '{raw}' to DateTime");
                return vDate;
            }

            if (underlying.IsEnum)
            {
                try
                {
                    return Enum.Parse(underlying, raw, ignoreCase: true);
                }
                catch
                {
                    throw new FormatException($"Cannot convert '{raw}' to enum {underlying.Name}");
                }
            }
            
            try
            {
                return Convert.ChangeType(raw, underlying);
            }
            catch (Exception ex)
            {
                throw new FormatException($"Cannot convert '{raw}' to {underlying.Name}", ex);
            }
        }
    }
}