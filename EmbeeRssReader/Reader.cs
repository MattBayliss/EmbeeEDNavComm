using EmbeeEDModel.Entities;
using EmbeeEDModel.Helpers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
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
        private string _feedCachePath;
        private Dictionary<string, RssEntry> _entries;
        private DateTime _lastCheckedOn;
        private TimeSpan _refreshInterval;

        private static NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        public Reader(string rssUrl, string cachePath, TimeSpan refreshInterval)
        {
            _rssUrl = rssUrl;
            _refreshInterval = refreshInterval;

            _feedCachePath = Path.Combine(cachePath, _rssUrl.ToFileName("json"));
            if (File.Exists(_feedCachePath))
            {
                var fileinfo = new FileInfo(_feedCachePath);
                _lastCheckedOn = fileinfo.LastWriteTime;
            }
            else
            {
                _lastCheckedOn = new DateTime(0);
            }

            _entries = null;
        }

        /// <summary>
        /// Clears all cached information - read status and saved feed json file
        /// </summary>
        public void ResetCache()
        {
            _entries = null;
            if (File.Exists(_feedCachePath))
            {
                try
                {
                    File.Delete(_feedCachePath);
                }
                catch (IOException ex)
                {
                    // failed to delete the file - may be locked? Log the error,
                    // and just leave all entries in memory for now
                    Logger.Error("Failed to delete RSS cache file " + _feedCachePath, ex);
                }
            }
        }

        public IEnumerable<RssEntry> GetUnread()
        {
            LoadEntries();

            return _entries.Values.Where(e => e.Read == false).OrderBy(e => e.DatePosted).ToList();
        }

        public IEnumerable<RssEntry> EntriesAfter(DateTime dateFrom)
        {
            LoadEntries();

            return _entries.Values.Where(e => e.DatePosted > dateFrom).OrderBy(e => e.DatePosted).ToList();
        }

        private void LoadEntries()
        {
            bool refresh = _lastCheckedOn.Add(_refreshInterval) < DateTime.Now;
            bool getentries = refresh;
            if (_entries == null)
            {
                getentries = true;
                _entries = new Dictionary<string, RssEntry>();
            }
            if(getentries) {
                var entries = GetEntries(refresh);
                foreach (var entry in entries)
                {
                    var id = entry.Id;
                    if (!_entries.ContainsKey(id))
                    {
                        _entries.Add(id, entry);
                    }
                }
            }
        }

        private IEnumerable<RssEntry> GetEntries(bool refresh)
        {
            XmlReader reader;
            var entries = new List<RssEntry>();

            // first get the one from the cache if it's there
            if (File.Exists(_feedCachePath) && (!refresh))
            {
                entries = GetCachedEntries().ToList();
            }
            else
            {
                reader = XmlReader.Create(_rssUrl);
                SyndicationFeed feed = SyndicationFeed.Load(reader);
                reader.Close();

                using (var sw = new StreamWriter(_feedCachePath))
                {

                    foreach (SyndicationItem item in feed.Items)
                    {
                        var entry = new RssEntry()
                        {
                            Id = item.Id,
                            Title = item.Title.Text,
                            DatePosted = item.PublishDate.DateTime,
                            Read = false
                        };

                        if (item.Content != null)
                        {
                            var tsc = (TextSyndicationContent)item.Content;
                            entry.Body = tsc.Text;
                            if (item.Summary != null)
                            {
                                entry.Abstract = item.Summary.Text;
                            }
                        }
                        else if (item.Summary != null)
                        {
                            entry.Body = item.Summary.Text;
                        }

                        entries.Add(entry);
                        sw.WriteLine(JsonConvert.SerializeObject(entry));
                    }
                    sw.Close();
                }
            }

            return entries;

        }

        private IEnumerable<RssEntry> GetCachedEntries()
        {
            var entries = new List<RssEntry>();
            try
            {
                using (var entryReader = new StreamReader(_feedCachePath))
                {
                    try
                    {
                        while (!entryReader.EndOfStream)
                        {
                            var entryline = entryReader.ReadLine();

                            try
                            {
                                var entry = JsonConvert.DeserializeObject<RssEntry>(entryline);
                                entries.Add(entry);
                            }
                            catch (JsonSerializationException jsonex)
                            {
                                Logger.Debug("Invalid line in {0}: {1}", _feedCachePath, entryline);
                                Logger.Debug(string.Empty, jsonex);
                            }
                        }
                    }
                    catch (IOException iioex)
                    {
                        Logger.Error("Error reading stars", iioex);
                    }
                    finally
                    {
                        entryReader.Close();
                    }
                }
            }
            catch (IOException ioex)
            {
                Logger.Error("Error reading stars", ioex);
            }
            return entries;
        }
    }
}
