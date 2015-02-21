using EmbeeEDModel.Entities;
using EmbeeRssReader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmbeeEDNavServer
{
    public class GalnetReader
    {
        private Reader _reader;

        public GalnetReader()
        {
            //make a config item
            _reader = new Reader("http://elitedangerous.com/news/galnet/rss", Config.GetAppDataPath(), TimeSpan.FromHours(1));
        }

        public IEnumerable<RssEntry> CheckForNews()
        {
            return _reader.GetUnread(true);
        }
    }
}
