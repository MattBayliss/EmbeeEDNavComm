using EmbeeEDModel.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Syndication;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace EmbeeRssReader
{
    public class Reader
    {
        private string _rssUrl;

        public Reader(string rssUrl)
        {
            _rssUrl = rssUrl;
        }

        public IEnumerable<RssEntry> EntriesAfter(DateTime dateFrom)
        {
            var entries = new List<RssEntry>();
            XmlReader reader = XmlReader.Create(_rssUrl, new XmlReaderSettings()
            {
            });
            SyndicationFeed feed = SyndicationFeed.Load(reader);
            reader.Close();
            foreach (SyndicationItem item in feed.Items)
            {
                entries.Add(new RssEntry()
                {
                    DatePosted = item.PublishDate.DateTime
                });
            }

            return entries;
        }
    }
}
