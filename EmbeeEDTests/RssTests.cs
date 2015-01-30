using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmbeeEDTests
{
    [TestClass]
    public class RssTests
    {
        private EmbeeRssReader.Reader reader;

        [TestInitialize]
        public void Init()
        {
            reader = new EmbeeRssReader.Reader("https://gonegeeky.com/edproxy/");
            //reader = new EmbeeRssReader.Reader("http://elitedangerous.com/news/galnet/rss");
        }

        [TestMethod]
        public void ReadLastFortnight()
        {
            var datefrom = DateTime.Now.Subtract(TimeSpan.FromDays(14));
            var entries = reader.EntriesAfter(datefrom);

            Assert.IsTrue(entries.Count() > 0);

            var lastdate = datefrom;
            foreach (var entry in entries)
            {
                Assert.IsTrue(entry.DatePosted > datefrom);
                Assert.IsTrue(entry.DatePosted >= lastdate);
                lastdate = entry.DatePosted;
            }
        }
    }
}
