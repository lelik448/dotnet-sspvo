using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace SsPvo.Ui.Common
{
    public static class XmlSerialization
    {
        public static void WriteToXmlFile<T>(string filePath, T objectToWrite, bool append = false)
            where T : new()
        {
            using (var writer = new StreamWriter(filePath, append))
            {
                var serializer = new XmlSerializer(typeof(T));
                serializer.Serialize(writer, objectToWrite);
            }
        }

        public static T ReadFromXmlFile<T>(string filePath)
            where T : new()
        {
            using (var reader = new StreamReader(filePath))
            {
                var serializer = new XmlSerializer(typeof(T));
                return (T)serializer.Deserialize(reader);
            }
        }

        public static void SerializeToUtf8Xml<T>(string filePath, T objectToWrite, bool omitDeclaration = false)
        {
            var serializer = new XmlSerializer(typeof(T));
            var ns = new XmlSerializerNamespaces();
            ns.Add("", "");
            var settings = new XmlWriterSettings()
            {
                ConformanceLevel = ConformanceLevel.Document,
                Encoding = new UTF8Encoding(false),
                Indent = true,
                OmitXmlDeclaration = omitDeclaration
            };
            using (var writer = XmlWriter.Create(filePath, settings))
            {
                serializer.Serialize(writer, objectToWrite, ns);
            }
        }
    }
}
