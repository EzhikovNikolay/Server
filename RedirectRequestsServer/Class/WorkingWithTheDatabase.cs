using RedirectRequestsServer.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedirectRequestsServer.Class
{
    internal static class WorkingWithTheDatabase
    {
        public static async Task<string> AddProduct(List<Product> productsToAdd)
        {
            try
            {
                string connectionString = $"Data Source={Program.ServerName};Initial Catalog={Program.DBName};Trusted_Connection=True;";
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    foreach (Product product in productsToAdd)
                    {
                        StringBuilder queryBuilder = new StringBuilder($"INSERT INTO [{Program.ProductTableName}] (");
                        var properties = typeof(Product).GetProperties()
                            .Where(p => p.Name != "ID").ToList();

                        foreach (var property in properties)
                        {
                            queryBuilder.Append($"{property.Name}, ");
                        }
                        queryBuilder.Length -= 2;
                        queryBuilder.Append(") VALUES (");
                        for (int i = 0; i < properties.Count; i++)
                        {
                            queryBuilder.Append($"@Value_{i + 1}, ");
                        }
                        queryBuilder.Length -= 2;
                        queryBuilder.Append(")");

                        using (SqlCommand command = new SqlCommand(queryBuilder.ToString(), connection))
                        {
                            int paramIndex = 1;
                            foreach (var property in properties)
                            {
                                object value = property.GetValue(product);
                                if (property.Name == "Photo" && value is string base64String)
                                {
                                    byte[] photoBytes = Convert.FromBase64String(base64String);
                                    command.Parameters.AddWithValue($"@Value_{paramIndex}", photoBytes);
                                }
                                else
                                {
                                    command.Parameters.AddWithValue($"@Value_{paramIndex}", value ?? DBNull.Value);
                                }
                                paramIndex++;
                            }
                            command.ExecuteNonQuery();
                        }
                    }
                }
                return "Продукт добавлен!";
            }
            catch (Exception ex)
            {
                return "Ошибка при добавлении!" + " " + ex;
            }
        }

        public static async Task<string> DeleteProduct(List<int> productIdsToDelete)
        {
            try
            {
                string connectionString = $"Data Source={Program.ServerName};Initial Catalog={Program.DBName};Trusted_Connection=True;";
                using (var connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    for (var i = 0; i < productIdsToDelete.Count; i++)
                    {
                        var productId = productIdsToDelete[i];
                        var queryBuilder = new StringBuilder($"DELETE FROM [{Program.ProductTableName}] WHERE ID=@Value_{i + 1}");
                        var command = new SqlCommand(queryBuilder.ToString(), connection);
                        command.Parameters.AddWithValue($"@Value_{i + 1}", productId);
                        await command.ExecuteNonQueryAsync();
                    }
                }
                return "Продукт удален!";
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return "Ошибка при удалении!";
            }
        }

        public static async Task<string> EditProduct(Product selectedProduct)
        {
            try
            {
                string connectionString = $"Data Source={Program.ServerName};Initial Catalog={Program.DBName};Trusted_Connection=True;";

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();
                    StringBuilder queryBuilder = new StringBuilder($"UPDATE [{Program.ProductTableName}] SET ");
                    queryBuilder.Append("VendorCode = @VendorCode, ");
                    queryBuilder.Append("Name = @Name, ");
                    queryBuilder.Append("Description = @Description, ");
                    queryBuilder.Append("Category = @Category, ");
                    queryBuilder.Append("Manufacturer = @Manufacturer, ");
                    queryBuilder.Append("Price = @Price, ");
                    queryBuilder.Append("Quantity = @Quantity ");

                    if (!string.IsNullOrEmpty(selectedProduct.Photo))
                    {
                        queryBuilder.Append(", Photo = @Photo ");
                    }
                    queryBuilder.Append("WHERE ID = @ID");

                    using (SqlCommand command = new SqlCommand(queryBuilder.ToString(), connection))
                    {
                        command.Parameters.AddWithValue("@VendorCode", selectedProduct.VendorCode ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@Name", selectedProduct.Name ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@Description", selectedProduct.Description ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@Category", selectedProduct.Category ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@Manufacturer", selectedProduct.Manufacturer ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@Price", selectedProduct.Price);
                        command.Parameters.AddWithValue("@Quantity", selectedProduct.Quantity);
                        command.Parameters.AddWithValue("@ID", selectedProduct.ID);

                        if (!string.IsNullOrEmpty(selectedProduct.Photo))
                        {
                            byte[] photoBytes = Convert.FromBase64String(selectedProduct.Photo);
                            command.Parameters.Add(new SqlParameter("@Photo", SqlDbType.VarBinary) { Value = photoBytes });
                        }
                        await command.ExecuteNonQueryAsync();
                    }
                }
                return "Продукт изменен!";
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return "Ошибка при изменении!";
            }
        }
    }
}
