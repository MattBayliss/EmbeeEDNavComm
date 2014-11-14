using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmbeeEDNavServer.Edsc
{
    public class DateTimeConverter : Newtonsoft.Json.Converters.DateTimeConverterBase
    {
        public override object ReadJson(Newtonsoft.Json.JsonReader reader, Type objectType, object existingValue, Newtonsoft.Json.JsonSerializer serializer)
        {
            if (reader.TokenType != JsonToken.String)
            {
                throw new Exception(
                    String.Format("Unexpected token parsing date. Expected String, got {0}.",
                    reader.TokenType));
            }

            var datestring = (string)reader.Value;
            DateTime date;
            if (DateTime.TryParse(datestring, out date))
            {
                return date;
            }
            else
            {
                throw new FormatException(string.Format("Bad DateTime format encountered: {0}", datestring));
            }
        }

        public override void WriteJson(Newtonsoft.Json.JsonWriter writer, object value, Newtonsoft.Json.JsonSerializer serializer)
        {
            if (value is DateTime)
            {
                writer.WriteValue(((DateTime)value).ToString("yyyy-MM-dd hh:mm:ss"));
            }
            else
            {
                throw new FormatException("Expected date object value.");
            }
        }
    }
}
