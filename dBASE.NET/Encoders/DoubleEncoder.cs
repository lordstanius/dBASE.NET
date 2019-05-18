using System;
using System.Globalization;
using System.Text;

namespace dBASE.NET.Encoders
{
    internal class DoubleEncoder : Encoder
    {
        public DoubleEncoder(Encoding encoding) : base(encoding) { }

        public override byte[] Encode(DbfField field, object data)
        {
            string format = $"{{0,{field.Length}:0.{new string('0', field.Precision)}}}";
            string text = string.Format(CultureInfo.InvariantCulture, format, data);
            if (text.Length > field.Length)
                text.Substring(0, field.Length);

            return this.Encoding.GetBytes(text);
        }

        public override object Decode(ArraySegment<byte> bytes, DbfMemo memo)
        {
            string text = Encoding.GetString(bytes.Array, bytes.Offset, bytes.Count).Trim();
            if (text.Length == 0)
                return null;

            return Convert.ToDouble(text, CultureInfo.InvariantCulture);
        }

        public override object Parse(string value)
        {
            return double.Parse(value, CultureInfo.InvariantCulture);
        }
    }
}
