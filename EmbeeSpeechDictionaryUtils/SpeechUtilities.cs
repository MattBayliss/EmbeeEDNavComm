using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpeechLib;
using System.Globalization;

namespace EmbeeSpeechDictionaryUtils
{
    public static class SpeechUtilities
    {
        public static void AddWords(IEnumerable<string> words)
        {
            if (words != null && words.Count() > 0)
            {
                var langid = CultureInfo.CurrentCulture.LCID;
                var lex = new SpLexicon();
                
                foreach (var word in words)
                {
                    lex.AddPronunciation(word, langid);
                }
                lex = null;
            }
        }
    }
}
