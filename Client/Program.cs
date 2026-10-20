using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using Server;

namespace Client
{
    internal class Program
    {
        const string host = "127.0.0.1";
        const int port = 5000;
        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;

            var manager = new ConsoleMenu();
            while (true)
            {
                Console.WriteLine("\n======== Меню ========");

                Console.WriteLine("\n========== АВТОСАЛОН ==========");                
                Console.WriteLine("1. Поиск автомобиля по Id");
                Console.WriteLine("2. Поиск по диапазону цен");
                Console.WriteLine("3. Поиск по марке и модели");
                Console.WriteLine("4. Снять автомобиль с продажи");
                Console.WriteLine("5. Добавить автомобиль");
                Console.WriteLine("6. Продать автомобиль");
                Console.WriteLine("7. Изменить параметры автомобиля");               

                Console.WriteLine("======================");

                Console.WriteLine("0. Выход\n");
                Console.Write("Выбор >> ");

                string choice = Console.ReadLine()?.Trim();
                switch (choice)
                {
                    case "1": manager.SearchById(); break;
                    case "2": manager.SearchPriceRange(); break;
                    case "3": manager.SearchBrandAndModel(); break;
                    case "4": manager.RemoveFromSale(); break;
                    case "5": manager.AddCar(); break;
                    case "6": manager.SellCar(); break;
                    case "7": manager.UpdateCar(); break;                   

                    case "0":
                        Console.WriteLine("ВЫХОД");
                        return;
                    default:                        
                        CarResponseFormatter.PrintYellow("Неверная команда");
                        break;
                }
            }
        }
        public static string SendCommand(string command)
        {
            try
            {
                Console.WriteLine($"Подключение к серверу {host}:{port}...");
                Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                client.Connect(host, port);
                CarResponseFormatter.PrintGreen("Все подключено");

                Console.WriteLine($"Команда: {command}");
                byte[] buffer = Encoding.UTF8.GetBytes(command);
                client.Send(buffer);
               
                byte[] receive = new byte[4096];
                int bytesReceive = client.Receive(receive);

                if (bytesReceive == 0)
                {
                    return "ERROR|Сервер не ответил"; 
                }

                string responce = Encoding.UTF8.GetString(receive, 0, bytesReceive);                
                Console.WriteLine("\n ---- ОТВЕТ СЕРВЕРА ----");
                return (responce).Trim();   
            }
            catch (Exception ex)
            {
                CarResponseFormatter.PrintError($"\nERROR клиента: {ex.Message}");
                return $"ERROR|Ошибка соединения: {ex.Message}";
            }
        }
    }
}
