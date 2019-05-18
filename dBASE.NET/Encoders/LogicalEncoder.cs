using System;
using System.Text;

namespace dBASE.NET.Encoders
{
    internal class LogicalEncoder : Encoder
    {
        public LogicalEncoder(Encoding encoding) : base(encoding) { }

        public override byte[] Encode(DbfField field, object data)
        {
            // Convert boolean value to string.
            string text = "?";
            if (data != null)
            {
                text = (bool)data == true ? "Y" : "N";
            }

            // Grow string to fill field length.
            text = text.PadLeft(field.Length, ' ');

            // Convert string to byte array.
            return Encoding.GetBytes(text);
        }

        public override object Decode(ArraySegment<byte> bytes, DbfMemo memo)
        {
            string text = Encoding.GetString(bytes.Array, bytes.Offset, bytes.Count).Trim().ToUpper();
            if (text == "?") return null;
            return (text == "Y" || text == "T");
        }

        public override object Parse(string value)
        {
            return bool.Parse(value);
        }
    }
}