using System.Data;
using System.Data.Common;
using System.Reflection;
using Microsoft.EntityFrameworkCore;

namespace Jarvis.Infrastructure.Database.EntityFramework;

public static partial class DbContextExtension
{
    public static async Task<List<T>> RawSqlQueryAsync<T>(this DbContext dbContext, string query, Dictionary<string, object> parameters, Func<DbDataReader, T> parser)
    {
        using (var command = dbContext.Database.GetDbConnection().CreateCommand())
        {
            command.CommandText = query;
            command.CommandType = CommandType.Text;

            dbContext.Database.OpenConnection();

            if (parameters != null)
            {
                foreach (KeyValuePair<string, object> p in parameters)
                {
                    DbParameter dbParameter = command.CreateParameter();
                    dbParameter.ParameterName = p.Key;
                    dbParameter.Value = p.Value;
                    command.Parameters.Add(dbParameter);
                }
            }

            using (var reader = await command.ExecuteReaderAsync())
            {
                var entities = new List<T>();
                while (reader.Read())
                {
                    entities.Add(parser(reader));
                }
                return entities;
            }
        }
    }

    public static List<T> RawSqlQuery<T>(this DbContext dbContext, string query, Dictionary<string, object> parameters)
    {
        List<T> items = new List<T>();
        using (var command = dbContext.Database.GetDbConnection().CreateCommand())
        {
            command.CommandText = query;
            command.CommandType = CommandType.Text;

            dbContext.Database.GetDbConnection().Open();

            if (parameters != null)
            {
                foreach (KeyValuePair<string, object> p in parameters)
                {
                    DbParameter dbParameter = command.CreateParameter();
                    dbParameter.ParameterName = p.Key;
                    dbParameter.Value = p.Value;
                    command.Parameters.Add(dbParameter);
                }
            }

            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    if (reader.FieldCount == 1)
                    {
                        var item = (T)reader[0];
                        items.Add(item);
                    }
                    else
                    {
                        T item = Activator.CreateInstance<T>();
                        foreach (PropertyInfo prop in item.GetType().GetProperties())
                        {
                            if (!object.Equals(reader[prop.Name], DBNull.Value))
                            {
                                prop.SetValue(item, reader[prop.Name], null);
                            }
                        }
                        items.Add(item);
                    }
                }
            }

            dbContext.Database.GetDbConnection().Close();
        }
        return items;
    }
}