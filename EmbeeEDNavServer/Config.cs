using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmbeeEDNavServer
{
    public static class Config
    {
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
