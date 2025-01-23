using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace RedirectRequestsServer.Class
{
    internal class QrCodeAuthorization
    {
        public static async Task<string> SendAuthorizationToWpf(string text)
        {
            try
            {
                Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                IPEndPoint remoteEndPoint = new IPEndPoint(IPAddress.Loopback, 1234);
                await socket.ConnectAsync(remoteEndPoint);
                byte[] data = Encoding.UTF8.GetBytes(text);
                await socket.SendAsync(new ArraySegment<byte>(data), SocketFlags.None);
                socket.Close();
                return "Пользователь авторизирован!";
            }
            catch
            {
                return "Ошибка авторизации!";
            }
        }
    }
}
