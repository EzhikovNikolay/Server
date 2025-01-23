using RedirectRequestsServer.Class;
using System;
using System.Configuration;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace RedirectRequestsServer
{
    class Program
    {
        public static int Port = 5800;
        public static string ServerName = "localhost";
        public static string DBName = "Warehouse";
        public static string UserTableName = "User";
        public static string ProductTableName = "Product";

        static async Task Main(string[] args)
        {
            TcpListener listener = new TcpListener(IPAddress.Any, Port);
            listener.Start();
            Console.WriteLine("\n" + "Сервер запущен" + "\n" + $"IP {IPAddress.Any}" + "\n" + $"Port {Port}");
            while (true)
            {
                var client = await listener.AcceptTcpClientAsync();
                _ = ProcessingRequest.HandleClientAsync(client);
            }
        }
    }
}
