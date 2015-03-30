using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmbeeEDNavServer
{
    public class Config
    {
        private Dictionary<string, string> _settings;
        private string _settingsPath;

        public Config()
        {
            _settingsPath = Path.Combine(GetAppDataPath(), "settings.json");
            _settings = new Dictionary<string, string>();

            try
            {
                if (File.Exists(_settingsPath))
                {
                    using (var sr = new StreamReader(_settingsPath))
                    {
                        while (!sr.EndOfStream)
                        {
                            var settingline = sr.ReadLine();
                            if (!string.IsNullOrEmpty(settingline))
                            {
                                var kvp = Newtonsoft.Json.JsonConvert.DeserializeObject<KeyValuePair<string, string>>(settingline);
                                _settings.Add(kvp.Key, kvp.Value);
                            }
                        }
                        sr.Close();
                    }
                }
            }
            catch {
                // can't load settings. That's cool.
            }
        }

        public string this[string key]
        {
            get
            {
                if (_settings.ContainsKey(key))
                {
                    return _settings[key];
                }
                else
                {
                    return null;
                }
            }
            set
            {
                if (_settings.ContainsKey(key))
                {
                    _settings[key] = value;
                }
                else
                {
                    _settings.Add(key, value);
                }
            }
        }

        public void Save()
        {
            using (var sw = new StreamWriter(_settingsPath, false))
            {
                foreach (var kvp in _settings)
                {
                    sw.WriteLine(JsonConvert.SerializeObject(kvp));
                }
                sw.Close();
            }
        }


        public static string GetAppDataPath()
        {
            var appfolder = Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "EmbeeED");
            if (!Directory.Exists(appfolder))
            {
                Directory.CreateDirectory(appfolder);
            }
            return appfolder;
        }


    }
}
