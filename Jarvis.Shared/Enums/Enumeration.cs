using System.Reflection;

namespace Jarvis.Shared.Enums;

public abstract class Enumeration : IComparable
{
    public string Name { get; private set; }

    public int Id { get; private set; }

    protected Enumeration(int id, string name)
    {
        (Id, Name) = (id, name);
    }

    public override string ToString() => Name;

    public static IEnumerable<T> GetAll<T>() where T : Enumeration
    {
        return typeof(T)
            .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .Select(f => f.GetValue(null))
            .Cast<T>();
    }

    public static string GetValue<T>(int id) where T : Enumeration
    {
        var item = GetAll<T>().FirstOrDefault(x => x.Id == id);
        if (item == null)
            return null;

        return item.Name;
    }

    public override bool Equals(object obj)
    {
        if (obj is not Enumeration otherValue)
            return false;

        var typeMatches = GetType().Equals(obj.GetType());
        var valueMatches = Id.Equals(otherValue.Id);

        return typeMatches && valueMatches;
    }

    public int CompareTo(object other)
    {
        return Id.CompareTo(((Enumeration)other).Id);
    }

    public override int GetHashCode()
    {
        return Id;
    }
}