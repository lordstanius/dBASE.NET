using System;
using System.Globalization;
using System.Text;

namespace dBASE.NET.Encoders
{
    internal abstract class Encoder
    {
        protected Encoding Encoding;

        public Encoder(Encoding encoding)
        {
            Encoding = encoding;
        }

        public abstract byte[] Encode(DbfField field, object data);

        public abstract object Decode(ArraySegment<byte> bytes, DbfMemo memo);

        public abstract object Parse(string value);

        public virtual string ToString(object value)
        {
            return Convert.ToString(value, CultureInfo.InvariantCulture);
        }

        public static string EscapeString(string text)
        {
            return text
                .Replace("\r", "\\r")
                .Replace("\n", "\\n");
        }

        public static string UnescapeString(string text)
        {
            return text
                .Replace("\\r", "\r")
                .Replace("\\n", "\n");
        }
    }
}
