using Newtonsoft.Json;
using RedirectRequestsServer.Model;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace RedirectRequestsServer.Class
{
    public static class ProcessingRequest
    {
        public static async Task HandleClientAsync(TcpClient client)
        {
            try
            {
                using (client)
                using (var stream = client.GetStream())
                {
                    var requestBuilder = new StringBuilder();
                    byte[] buffer = new byte[8192];
                    int bytesRead;

                    while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                    {
                        string requestPart = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                        requestBuilder.Append(requestPart);

                        // Если сообщение заканчивается на "\n", то обрабатываем его
                        if (requestPart.EndsWith("\n"))
                        {
                            string completeRequest = requestBuilder.ToString().TrimEnd('\n');
                            await ProcessRequestAsync(completeRequest, stream);
                            requestBuilder.Clear(); // Очищаем для следующего сообщения
                        }
                    }
                }
            }
            catch (OutOfMemoryException ex)
            {
                Console.WriteLine($"Out of memory: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        private static async Task ProcessRequestAsync(string request, NetworkStream stream)
        {
            if (request.StartsWith("DeleteProduct:"))
            {
                await HandleDeleteProductAsync(request, stream);
            }
            else if (request.StartsWith("EditProduct:"))
            {
                await HandleEditProductAsync(request, stream);
            }
            else if (request.StartsWith("AddProduct:"))
            {
                await HandleAddProductAsync(request, stream);
            }
            else if (request.Contains("Registration"))
            {
                await HandleUserRegistrationAsync(request, stream);
            }
            else if (request.Contains("Give"))
            {
                await HandleDataRequestAsync(stream);
            }
            else if (request.Contains("EditUserData"))
            {
                await HandleEditProfileAsync(request, stream);
            }
            else if (request.Contains("QrAutorization:"))
            {
                await HandleQrAutorizationAsync(request, stream);
            }
            else
            {
                await HandleUseAuthorizationAsync(request, stream);
            }
        }
        private static async Task HandleQrAutorizationAsync(string request, NetworkStream stream)
        {
            string message = request.Substring("QrAutorization:".Length);
            if (message != null)
            {
                string responseMessage = await QrCodeAuthorization.SendAuthorizationToWpf(message);
                await SendResponseAsync(responseMessage, stream);
            }
        }

        private static async Task HandleEditProfileAsync(string request, NetworkStream stream)
        {
            string userData = request.Substring("EditUserData:".Length);
            if (userData != null)
            {
                string responseMessage = await UserInteraction.EditUserProfile(userData);
                await SendResponseAsync(responseMessage, stream);
            }
        }

        private static async Task HandleDeleteProductAsync(string request, NetworkStream stream)
        {
            string jsonProduct = request.Substring("DeleteProduct:".Length);
            if (jsonProduct != null)
            {
                string responseMessage = await WorkingWithTheDatabase.DeleteProduct(new List<int> { int.Parse(jsonProduct) });
                await SendResponseAsync(responseMessage, stream);
            }
        }

        private static async Task HandleEditProductAsync(string request, NetworkStream stream)
        {
            string jsonProduct = request.Substring("EditProduct:".Length);
            var productToEdit = JsonConvert.DeserializeObject<Product>(jsonProduct);
            if (productToEdit != null)
            {
                string responseMessage = await WorkingWithTheDatabase.EditProduct(productToEdit);
                await SendResponseAsync(responseMessage, stream);
            }
        }

        private static async Task HandleAddProductAsync(string request, NetworkStream stream)
        {
            string jsonProduct = request.Substring("AddProduct:".Length);
            var productToAdd = JsonConvert.DeserializeObject<Product>(jsonProduct);
            if (productToAdd != null)
            {
                string responseMessage = await WorkingWithTheDatabase.AddProduct(new List<Product> { productToAdd });
                await SendResponseAsync(responseMessage, stream);
            }
        }

        private static async Task HandleUserRegistrationAsync(string request, NetworkStream stream)
        {
            string response = await UserInteraction.RegistrationUser(request);
            await SendResponseAsync(response, stream);
        }

        private static async Task HandleUseAuthorizationAsync(string request, NetworkStream stream)
        {
            string response = await UserInteraction.AuthorizationUser(request);
            await SendResponseAsync(response, stream);
        }

        private static async Task HandleDataRequestAsync(NetworkStream stream)
        {
            List<Product> response = await DataLoader.LoadData();
            string jsonResponse = JsonConvert.SerializeObject(response);
            await SendResponseAsync(jsonResponse, stream);
        }
        private static async Task SendResponseAsync(string responseMessage, NetworkStream stream)
        {
            string completeResponse = responseMessage + "\n";
            byte[] responseBytes = Encoding.UTF8.GetBytes(completeResponse);
            int bufferSize = 8192;
            int offset = 0;
            while (offset < responseBytes.Length)
            {
                int bytesToWrite = Math.Min(bufferSize, responseBytes.Length - offset);
                await stream.WriteAsync(responseBytes, offset, bytesToWrite);
                offset += bytesToWrite;
            }
            await stream.FlushAsync();
        }
    }
}
