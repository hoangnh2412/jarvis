using Oracle.ManagedDataAccess.Client;
using System;
using System.Data;
using System.Threading.Tasks;

namespace Einvoice.Adapters.Oracle.Common.Extensions
{
    public static class DapperOracleExtension
    {
        public static async Task<T> QueryWrapperAsync<T>(string strConnection, Func<OracleConnection, Task<T>> handle)
        {
            using var connection = new OracleConnection(strConnection);
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

        public static async Task<int> ExecuteWrapperAsync(string strConnection, Func<OracleConnection, Task<int>> handle)
        {
            using var connection = new OracleConnection(strConnection);
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
