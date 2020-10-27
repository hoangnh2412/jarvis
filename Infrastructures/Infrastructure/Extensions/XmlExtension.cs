using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace Infrastructure.Extensions
{
    public static class XmlExtension
    {
        public static T XmlDeserialize<T>(this string input) where T : class
        {
            var ser = new XmlSerializer(typeof(T));

            using (var sr = new StringReader(input))
            {
                return (T)ser.Deserialize(sr);
            }
        }

        public static string XmlSerialize<T>(this T obj, bool omitXmlDeclaration = false)
        {
            var emptyNamepsaces = new XmlSerializerNamespaces(new[] { XmlQualifiedName.Empty });
            var serializer = new XmlSerializer(obj.GetType());
            var setting = new XmlWriterSettings
            {
                Indent = true,
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
    }
}
