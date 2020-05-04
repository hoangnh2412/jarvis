using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Reflection;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Database.EntityFramework
{
    public static class DbContextExtension
    {
        public static List<T> RawSqlQuery<T>(this DbContext dbContext, string query, Func<DbDataReader, T> map)
        {
            using (var command = dbContext.Database.GetDbConnection().CreateCommand())
            {
                command.CommandText = query;
                command.CommandType = CommandType.Text;

                dbContext.Database.OpenConnection();

                using (var result = command.ExecuteReader())
                {
                    var entities = new List<T>();

                    while (result.Read())
                    {
                        entities.Add(map(result));
                    }

                    return entities;
                }
            }
        }

        public static List<T> RawSqlQuery<T>(this DbContext dbContext, string query)
        {
            List<T> items = new List<T>();
            using (var command = dbContext.Database.GetDbConnection().CreateCommand())
            {
                command.CommandText = query;
                command.CommandType = CommandType.Text;

                dbContext.Database.OpenConnection();

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
            }
            return items;
        }
    }
}