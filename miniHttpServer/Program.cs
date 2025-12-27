using System.Text.Json;
using HttpServer.Shared;
using MiniHttpServer.Framework;

public class Program
{
    public static async Task Main(string[] args)
    {   
        
        CancellationTokenSource cts = new CancellationTokenSource();
        CancellationToken token = cts.Token;

        await Task.Run(() =>
        {
            try
            {
                var settings = SettingsModelSingleton.Instance;
                
                if (settings == null)
                {
                    Console.WriteLine("Ошибка: настройки не загружены");
                    return;
                }

                if (!System.IO.Directory.Exists("static"))
                    Console.WriteLine("Папка static не найдена");
                

                var server = new miniHttpServer.Framework.HttpServer(settings);


                Console.WriteLine($"Запуск сервера на {settings.Domain}:{settings.Port}/tour");
                server.Start(token);

                Console.WriteLine("Сервер запущен. Введите '/stop' для остановки");
                while (!token.IsCancellationRequested)
                {
                    var input = Console.ReadLine();
                    if (input == "/stop")
                    {
                        Console.WriteLine("Получена команда остановки...");
                        cts.Cancel();
                        break;
                    }
                }

                server.Stop();
                Console.WriteLine("Сервер остановлен");
            }
            catch (FileNotFoundException)
            {
                Console.WriteLine("Файл settings.json не найден");
                return;
            }
            catch (JsonException)
            {
                Console.WriteLine("Ошибка формата JSON");
                return;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Неожиданная ошибка: {ex.Message}");
                return;
            }
        });
    }
}