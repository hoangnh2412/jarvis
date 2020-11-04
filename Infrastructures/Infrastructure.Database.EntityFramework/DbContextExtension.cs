using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Database.EntityFramework
{
    public static class DbContextExtension
    {
        public static async Task<List<T>> RawSqlQueryAsync<T>(this DbContext dbContext, string query, Func<DbDataReader, T> map)
        {
            using (var command = dbContext.Database.GetDbConnection().CreateCommand())
            {
                command.CommandText = query;
                command.CommandType = CommandType.Text;

                await dbContext.Database.OpenConnectionAsync();

                using (var result = await command.ExecuteReaderAsync())
                {
                    var entities = new List<T>();
                    while (await result.ReadAsync())
                    {
                        entities.Add(map(result));
                    }

                    return entities;
                }
            }
        }

        public static async Task<List<T>> RawSqlQuery<T>(this DbContext dbContext, string query)
        {
            List<T> items = new List<T>();
            using (var command = dbContext.Database.GetDbConnection().CreateCommand())
            {
                command.CommandText = query;
                command.CommandType = CommandType.Text;

                await dbContext.Database.OpenConnectionAsync();

                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
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
            }
            return items;
        }
    }
}