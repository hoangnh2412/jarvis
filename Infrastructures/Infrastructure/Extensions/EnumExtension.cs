using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Infrastructure.Extensions
{
    public static class EnumExtension
    {
        public static string GetName(this Enum value)
        {
            var items = value.GetType().GetMember(value.ToString());
            if (items.Length == 0)
                return null;

            var item = items.First().GetCustomAttribute<DisplayAttribute>();
            if (item == null)
                return items.First().GetCustomAttribute<DisplayNameAttribute>().DisplayName;
            else
                return item.GetName();
        }

        public static string GetShortName(this Enum value)
        {
            var items = value.GetType().GetMember(value.ToString());
            if (items.Length == 0)
                return null;

            var item = items.First().GetCustomAttribute<DisplayAttribute>();
            if (item == null)
                return null;
            return item.GetShortName();
        }

        public static string GetDescription(this Enum value)
        {
            var items = value.GetType().GetMember(value.ToString());
            if (items.Length == 0)
                return null;

            var item = items.First().GetCustomAttribute<DisplayAttribute>();
            if (item == null)
                return null;
            return item.GetDescription();
        }

        public static string GetGroupName(this Enum value)
        {
            var items = value.GetType().GetMember(value.ToString());
            if (items.Length == 0)
                return null;

            var item = items.First().GetCustomAttribute<DisplayAttribute>();
            if (item == null)
                return null;
            return item.GetGroupName();
        }

        public static Dictionary<int, string> ToDictionaryByHashCode<T>()
        {
            var items = GetValues<T>();
            var results = new Dictionary<int, string>();
            foreach (var item in items)
            {
                var key = item.GetHashCode();
                var members = item.GetType().GetMember(item.ToString());
                if (members.Length == 0)
                    throw new ArgumentNullException("Display Attribute is null");

                string value;
                var member = members.First().GetCustomAttribute<DisplayNameAttribute>();
                if (member == null)
                    value = members.First().GetCustomAttribute<DisplayAttribute>().GetName();
                else
                    value = members.First().GetCustomAttribute<DisplayNameAttribute>().DisplayName;

                results.Add(key, value);
            }
            return results;
        }

        public static IEnumerable<T> GetValues<T>()
        {
            return Enum.GetValues(typeof(T)).Cast<T>();
        }

        public static T ToEnum<T>(string value)
        {
            return (T)Enum.Parse(typeof(T), value, true);
        }

        public static T ToEnum<T>(int code)
        {
            return (T)Enum.Parse(typeof(T), code.ToString(), true);
        }

        public static T TryToEnum<T>(string value)
        {
            try
            {
                return (T)Enum.Parse(typeof(T), value, true);
            }
            catch (Exception)
            {
                return default(T);
            }
        }

        public static T TryToEnum<T>(int code)
        {
            try
            {
                return (T)Enum.Parse(typeof(T), code.ToString(), true);
            }
            catch (Exception)
            {
                return default(T);
            }
        }

        public static Dictionary<string, string> ToDictionary<T>()
        {
            var items = GetValues<T>();
            var results = new Dictionary<string, string>();
            foreach (var item in items)
            {
                var key = item.ToString();
                var members = item.GetType().GetMember(key);
                if (members.Length == 0)
                    throw new ArgumentNullException("Display Attribute is null");

                string value;
                var member = members.First().GetCustomAttribute<DisplayNameAttribute>();
                if (member == null)
                    value = members.First().GetCustomAttribute<DisplayAttribute>().GetName();
                else
                    value = members.First().GetCustomAttribute<DisplayNameAttribute>().DisplayName;

                results.Add(key, value);
            }
            return results;
        }

        public static KeyValuePair<bool, string>[] ToTrueFalse<T>()
        {
            var items = GetValues<T>();
            var results = new Dictionary<bool, string>();
            foreach (var item in items)
            {
                var key = item.ToString() == "Active";
                var members = item.GetType().GetMember(item.ToString());
                if (members.Length == 0)
                    throw new ArgumentNullException("Display Attribute is null");

                string value;
                var member = members.First().GetCustomAttribute<DisplayNameAttribute>();
                if (member == null)
                    value = members.First().GetCustomAttribute<DisplayAttribute>().GetName();
                else
                    value = members.First().GetCustomAttribute<DisplayNameAttribute>().DisplayName;

                results.Add(key, value);
            }
            return results.ToArray();
        }

        public static List<string> GetAllValues<T>()
        {
            var result = new List<string>();
            foreach (string value in Enum.GetNames(typeof(T)))
                result.Add(value);

            return result;
        }
    }

}
