

using System.Net;
using HttpServer.Framework.core.handlers;
using HttpServer.Shared;
using MiniHttpServer.Framework.Core.Abstracts;
using MiniHttpServer.Framework.Core.Handlers;
using MiniHttpServer.Shared;

namespace miniHttpServer.Framework
{

    public class HttpServer
    {
        private HttpListener _listener = new();
        private SettingsModel _config;
        private CancellationToken _token;

        public HttpServer(SettingsModel config) { _config = config; }

        public void Start(CancellationToken token)
        {
            _token = token;
            _listener = new HttpListener();
            string host = _config.Domain;
            string url = _config.Prefix; // берём ПРЯМО prefix
            _listener.Prefixes.Add(url);
            _listener.Start();
            Console.WriteLine("Сервер запущен! Проверяй в браузере: " + url + "tour");
            Receive();
        }

        public void Stop()
        {
            _listener.Stop();
        }

        private void Receive()
        {
            _listener.BeginGetContext(new AsyncCallback(ListenerCallback), _listener);
        }

        protected async void ListenerCallback(IAsyncResult result)
        {
            try
            {
                if (!_listener.IsListening || _token.IsCancellationRequested)
                    return;

                HttpListenerContext context;

                try
                {
                    context = _listener.EndGetContext(result);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Ошибка при получении HTTP-контекста: " + ex.Message);
                    if (!_token.IsCancellationRequested)
                        Receive();
                    return;
                }

                try
                {
                    Handler staticFiles = new StaticFilesHandler();
                    Handler endpoints = new EndpointsHandler();
                    staticFiles.Successor = endpoints;

                    staticFiles.HandleRequest(context);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Ошибка обработки запроса: " + ex.Message);

                    try
                    {
                        context.Response.StatusCode = 500;
                        context.Response.ContentType = "text/html; charset=utf-8";

                        byte[] msg = System.Text.Encoding.UTF8.GetBytes(
                            "<h1>500 — Внутренняя ошибка сервера</h1>"
                        );

                        context.Response.OutputStream.Write(msg, 0, msg.Length);
                        context.Response.Close();
                    }
                    catch { }
                }

                if (!_token.IsCancellationRequested)
                    Receive();
            }
            catch (Exception fatal)
            {
                Console.WriteLine("КРИТИЧЕСКАЯ ОШИБКА: " + fatal.Message);

                if (_listener.IsListening)
                    Receive();
            }
        }
        public static void SendStaticResponse(HttpListenerContext context, HttpStatusCode statusCode, string path)
        {
            var response = context.Response;
            var request = context.Request;

            response.StatusCode = (int)statusCode;
            response.ContentType = ContentType.GetContentType(path);

            var buffer = GetResponseBytes .Invoke(path);
            response.ContentLength64 = buffer.Length;

            using var output = response.OutputStream;
            output.Write(buffer, 0, buffer.Length);


            if (response.StatusCode == 200)
                Console.WriteLine(
                    $"Запрос обработан: {request.Url.AbsolutePath} {request.HttpMethod} - Status: {response.StatusCode}");
            else
                Console.WriteLine(
                    $"Ошибка запроса: {request.Url.AbsolutePath} {request.HttpMethod} - Status: {response.StatusCode}");
        }

    }
}