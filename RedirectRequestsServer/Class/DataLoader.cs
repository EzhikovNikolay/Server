using RedirectRequestsServer.Model;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace RedirectRequestsServer.Class
{
    internal class DataLoader
    {
        public static async Task<List<Product>> LoadData()
        {
            try
            {
                string connectionString = $"Data Source={Program.ServerName};Initial Catalog={Program.DBName};Trusted_Connection=True;";

                using (var connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();
                    string query = $"SELECT * FROM [{Program.ProductTableName}]";
                    SqlCommand command = new SqlCommand(query, connection);
                    SqlDataReader reader = await command.ExecuteReaderAsync();
                    var products = new List<Product>();
                    while (await reader.ReadAsync())
                    {
                        var product = new Product();
                        product.ID = reader.GetInt32(0);
                        product.VendorCode = reader.GetString(1);

                        // Обработка изображения
                        if (!reader.IsDBNull(2))
                        {
                            byte[] photo = null;
                            var bytesRead = reader.GetBytes(2, 0, null, 0, 0);
                            photo = new byte[bytesRead];
                            reader.GetBytes(2, 0, photo, 0, (int)bytesRead);
                            product.Photo = Convert.ToBase64String(photo); // Конвертация в Base64
                        }
                        else
                        {
                            product.Photo = null; // Или присвойте значение по умолчанию
                        }

                        product.Name = reader.GetString(3);
                        product.Description = reader.GetString(4);
                        product.Category = reader.GetString(5);
                        product.Manufacturer = reader.GetString(6);
                        product.Price = reader.GetDecimal(7);
                        product.Quantity = reader.GetInt32(8);

                        products.Add(product);
                    }
                    reader.Close();
                    return products;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка загрузки данных: {ex.Message}");
                return null;
            }
        }
    }
}
