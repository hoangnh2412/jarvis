using System.Xml;
using System.Xml.Serialization;

namespace Jarvis.Shared.Extensions;

public static partial class XmlExtension
{
    public static string XmlSerialize<T>(this T obj, bool omitXmlDeclaration = false, bool indent = false)
    {
        var emptyNamepsaces = new XmlSerializerNamespaces(new[] { XmlQualifiedName.Empty });
        var serializer = new XmlSerializer(obj.GetType());
        var setting = new XmlWriterSettings
        {
            Indent = indent,
            OmitXmlDeclaration = omitXmlDeclaration
        };

        string xml;
        using (var stream = new StringWriter())
        {
            using (var writer = XmlWriter.Create(stream, setting))
            {
                serializer.Serialize(writer, obj, emptyNamepsaces);
                xml = stream.ToString();
            }
        }
        return xml;
    }

    public static T XmlDeserialize<T>(this string input) where T : class
    {
        var ser = new XmlSerializer(typeof(T));

        using (var sr = new StringReader(input))
        {
            return (T)ser.Deserialize(sr);
        }
    }

    public static XmlElement ToXmlElement(this string data)
    {
        var doc = new XmlDocument();
        doc.LoadXml(data);
        return doc.DocumentElement;
    }

    public static XmlDocument ToXmlDocument(this string data)
    {
        var doc = new XmlDocument();
        doc.LoadXml(data);
        return doc;
    }
}