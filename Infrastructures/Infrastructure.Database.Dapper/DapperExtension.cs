using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace Infrastructure.Database.Dapper
{
    public static class DapperExtension
    {
        public static T QueryWrapper<T>(string strConnection, Func<SqlConnection, T> handle)
        {
            using var connection = new SqlConnection(strConnection);
            try
            {
                if (connection.State == ConnectionState.Closed)
                    connection.Open();

                return handle(connection);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (connection.State == ConnectionState.Open)
                    connection.Close();
            }
        }

        public static int ExecuteWrapper(string strConnection, Func<SqlConnection, int> handle)
        {
            using var connection = new SqlConnection(strConnection);
            try
            {
                if (connection.State == ConnectionState.Closed)
                    connection.Open();

                return handle(connection);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (connection.State == ConnectionState.Open)
                    connection.Close();
            }
        }





        public static async Task<T> QueryWrapperAsync<T>(string strConnection, Func<SqlConnection, Task<T>> handle)
        {
            using var connection = new SqlConnection(strConnection);
            try
            {
                if (connection.State == ConnectionState.Closed)
                    connection.Open();

                return await handle(connection);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (connection.State == ConnectionState.Open)
                    connection.Close();
            }
        }

        public static async Task<int> ExecuteWrapperAsync(string strConnection, Func<SqlConnection, Task<int>> handle)
        {
            using var connection = new SqlConnection(strConnection);
            try
            {
                if (connection.State == ConnectionState.Closed)
                    connection.Open();

                return await handle(connection);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (connection.State == ConnectionState.Open)
                    connection.Close();
            }
        }
    }
}
