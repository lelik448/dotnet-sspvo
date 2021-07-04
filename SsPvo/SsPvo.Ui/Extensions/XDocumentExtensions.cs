using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace SsPvo.Ui.Extensions
{
    public static class XDocumentExtensions
    {
        public static byte[] ToUtf8ByteArray(this XDocument source)
        {
            var settings = new XmlWriterSettings()
            {
                ConformanceLevel = ConformanceLevel.Document,
                Encoding = new UTF8Encoding(false),
                Indent = true
                //OmitXmlDeclaration = omitDeclaration
            };

            using (var stream = new MemoryStream())
            using (var writer = XmlWriter.Create(stream, settings))
            {
                source.WriteTo(writer);
                writer.Close();
                return stream.ToArray();
            }
        }
    }
}
