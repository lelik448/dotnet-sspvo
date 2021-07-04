using System.IO;
using System.Text;

namespace SsPvo.Ui.Common
{
    public class Utf8StringWriter : StringWriter
    {
        public override Encoding Encoding => Encoding.UTF8;
    }
}
