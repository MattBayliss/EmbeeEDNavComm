using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmbeeEDNav
{
    public static class Extensions
    {
        public static void SetValue<T>(this Dictionary<string, T> dict, string key, T value)
        {
            if (dict.ContainsKey(key))
            {
                dict[key] = value;
            }
            else
            {
                dict.Add(key, value);
            }
        }

        public static T GetValue<T>(this Dictionary<string, T> dict, string key, T defaultValue)
        {
            if (dict.ContainsKey(key))
            {
                return dict[key];
            }
            else
            {
                return defaultValue;
            }
        }
    }
}
