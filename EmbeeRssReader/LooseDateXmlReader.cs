using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace EmbeeRssReader
{
    public class LooseDateXmlReader : XmlTextReader
    {
        private bool _readingDate;
        private const string CustomUtcDateTimeFormat = "ddd MMM dd HH:mm:ss Z yyyy"; // Wed Oct 07 08:00:07 GMT 2009

        public override void ReadStartElement()
        {
            _readingDate = (string.Equals(base.NamespaceURI, string.Empty, StringComparison.InvariantCultureIgnoreCase) &&
                     (string.Equals(base.LocalName, "lastBuildDate", StringComparison.InvariantCultureIgnoreCase) ||
                     string.Equals(base.LocalName, "pubDate", StringComparison.InvariantCultureIgnoreCase)));

            base.ReadStartElement();
        }
        public override void ReadEndElement()
        {
            _readingDate = false;
            base.ReadEndElement();
        }
        public override string ReadString()
        {
            if (_readingDate)
            {
                var dateString = base.ReadString();
                DateTime dt;
                if (!DateTime.TryParse(dateString, out dt))
                    dt = DateTime.ParseExact(dateString, CustomUtcDateTimeFormat, CultureInfo.InvariantCulture);
                return dt.ToUniversalTime().ToString("R", CultureInfo.InvariantCulture);
            }
            else
            {
                return base.ReadString();
            }
        }
    }
}
