using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmbeeEDModel.Helpers
{
    public static class StringHelpers
    {
        public static string ToFileName(this string input, string extension)
        {
            if (string.IsNullOrEmpty(input))
            {
                return input;
            }
            var safebits = input.Split(Path.GetInvalidFileNameChars());
            return string.Join(string.Empty, safebits) + "." + extension;
        }
    }
}
