using System;
using System.Globalization;
using System.Text;

namespace dBASE.NET.Encoders
{
    internal class DateEncoder : Encoder
    {
        public DateEncoder(Encoding encoding) : base(encoding) { }

        public override byte[] Encode(DbfField field, object data)
        {
            string text = new string(' ', field.Length);
            if (data != null)
            {
                DateTime dt = (DateTime)data;
                text = string.Format("{0:d4}{1:d2}{2:d2}", dt.Year, dt.Month, dt.Day).PadLeft(field.Length, ' ');
            }

            return Encoding.GetBytes(text);
        }

        public override object Decode(ArraySegment<byte> bytes, DbfMemo memo)
        {
            string text = Encoding.GetString(bytes.Array, bytes.Offset, bytes.Count).Trim();
            if (text.Length == 0) return null;
            return DateTime.ParseExact(text, "yyyyMMdd", CultureInfo.InvariantCulture);
        }

        public override object Parse(string value)
        {
            return DateTime.ParseExact(value, "yyyyMMdd", CultureInfo.InvariantCulture);
        }

        public override string ToString(object value)
        {
            var dt = (DateTime)value;

            return string.Format("{0:d4}{1:d2}{2:d2}", dt.Year, dt.Month, dt.Day);
        }
    }
}

