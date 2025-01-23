using System;
using System.Data.SqlClient;
using System.Text;
using System.Threading.Tasks;

namespace RedirectRequestsServer.Class
{
    internal class UserInteraction
    {
        public static async Task<string> RegistrationUser(string query)
        {
            string[] partQuery = query.Split('|');
            string connectionString = $"Data Source={Program.ServerName};Initial Catalog={Program.DBName};Trusted_Connection=True;";

            string queryString = $"INSERT INTO [{Program.UserTableName}] (Login, Password, Role, Surname, Name, Patronymic, PhoneNumber, Email, Education,Photo) VALUES (@Login, @Password, @Role, @Surname, @Name, @Patronymic, @PhoneNumber, @Email, @Education,@Photo)";
            byte[] queryBytes = Encoding.UTF8.GetBytes(partQuery[20]);
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                using (SqlCommand command = new SqlCommand(queryString, connection))
                {
                    try
                    {
                        command.Parameters.AddWithValue(partQuery[1], partQuery[2]);
                        command.Parameters.AddWithValue(partQuery[3], BCrypt.Net.BCrypt.HashPassword(partQuery[4]));
                        command.Parameters.AddWithValue(partQuery[5], partQuery[6]);
                        command.Parameters.AddWithValue(partQuery[7], partQuery[8]);
                        command.Parameters.AddWithValue(partQuery[9], partQuery[10]);
                        command.Parameters.AddWithValue(partQuery[11], partQuery[12]);
                        command.Parameters.AddWithValue(partQuery[13], partQuery[14]);
                        command.Parameters.AddWithValue(partQuery[15], partQuery[16]);
                        command.Parameters.AddWithValue(partQuery[17], partQuery[18]);
                        command.Parameters.AddWithValue(partQuery[19], queryBytes);

                        await command.ExecuteNonQueryAsync();
                        return "Пользователь зарегистрирован";
                    }
                    catch
                    {
                        return "Ошибка при регистрации";
                    }
                }
            }
        }
        public static async Task<string> AuthorizationUser(string query)
        {
            try
            {
                string[] partQuery = query.Split('|');
                string connectionString = $"Data Source={Program.ServerName};Initial Catalog={Program.DBName};Trusted_Connection=True;";

                using (var connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();
                    var command = new SqlCommand($"SELECT ID, Login, Password, Role, Surname, Name, Patronymic, PhoneNumber, Email, Education, Photo FROM [{Program.UserTableName}] WHERE Login = @Login", connection);
                    command.Parameters.AddWithValue("@Login", partQuery[0]);

                    var reader = await command.ExecuteReaderAsync(); // Заменил на ExecuteReaderAsync
                    if (reader.HasRows)
                    {
                        while (await reader.ReadAsync())
                        {
                            int UserID = reader.GetInt32(0);
                            string UserLogin = reader.GetString(1);
                            string UserPassword = reader.GetString(2);
                            string UserRole = reader.GetString(3);
                            string UserSurname = reader.GetString(4);
                            string UserName = reader.GetString(5);
                            string UserPatronymic = reader.GetString(6);
                            string UserPN = reader.GetString(7);
                            string UserEmail = reader.GetString(8);
                            string UserEAE = reader.GetString(9);
                            byte[] Photo;
                            if (!reader.IsDBNull(10))
                            {
                                var bytesRead = reader.GetBytes(10, 0, null, 0, 0);
                                Photo = new byte[bytesRead];
                                reader.GetBytes(10, 0, Photo, 0, (int)bytesRead);
                            }
                            else
                            {
                                Photo = null;
                            }
                            if (BCrypt.Net.BCrypt.Verify(partQuery[1], UserPassword))
                            {
                                string photoBase64 = Photo != null ? Convert.ToBase64String(Photo) : ""; // Добавлена проверка на null
                                return $"{UserID.ToString()}|{UserLogin}|{UserPassword}|{UserRole}|{UserSurname}|{UserName}|{UserPatronymic}|{UserPN}|{UserEmail}|{UserEAE}|{photoBase64}";
                            }
                            else
                            {
                                 reader.Close(); // Заменил на CloseAsync
                                return "Ошибка: Неправильный пароль";
                            }
                        }
                    }
                    else
                    {
                         reader.Close(); // Заменил на CloseAsync
                        return "Ошибка: Неправильный логин";
                    }
                }
            }
            catch (Exception ex)
            {
                return $"Ошибка подключения {ex}";
            }
            return null;
        }
        public static async Task<string> EditUserProfile(string response)
        {
            string connectionString = $"Data Source={Program.ServerName};Initial Catalog={Program.DBName};Trusted_Connection=True;";

            using (var connection = new SqlConnection(connectionString))
            {
                try
                {
                    string[] parts = response.Split('|');
                    await connection.OpenAsync();
                    var query = $"UPDATE [{Program.UserTableName}] SET Login = @Login, Password = @Password, Surname = @Surname, Name = @Name, Patronymic = @Patronymic, PhoneNumber = @PhoneNumber, Email = @Email, Education = @Education WHERE ID = @ID";
                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Login", parts[5]);
                        command.Parameters.AddWithValue("@Password", BCrypt.Net.BCrypt.HashPassword(parts[6]));
                        command.Parameters.AddWithValue("@Surname", parts[1]);
                        command.Parameters.AddWithValue("@Name", parts[2]);
                        command.Parameters.AddWithValue("@Patronymic", parts[3]);
                        command.Parameters.AddWithValue("@PhoneNumber", parts[7]);
                        command.Parameters.AddWithValue("@Email", parts[4]);
                        command.Parameters.AddWithValue("@Education", parts[8]);
                        command.Parameters.AddWithValue("@ID", parts[0]);
                        command.ExecuteNonQuery();
                    }

                    return "Данные изменены!";
                }
                catch (Exception ex)
                {
                    return ex.Message;
                }
            }
        }
    }
}
