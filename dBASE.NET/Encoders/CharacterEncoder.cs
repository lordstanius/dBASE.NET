using System;
using System.Text;

namespace dBASE.NET.Encoders
{
    internal class CharacterEncoder : Encoder
    {
        public CharacterEncoder(Encoding encoding) : base(encoding) { }

        public override byte[] Encode(DbfField field, object data)
        {
            // Convert data to string. NULL is the empty string.
            string text = data == null ? "" : (string)data;
            // Pad string with spaces.
            // Convert string to byte array.
            return Encoding.GetBytes(text.PadRight(field.Length));
        }

        public override object Decode(ArraySegment<byte> bytes, DbfMemo memo)
        {
            string text = Encoding.GetString(bytes.Array, bytes.Offset, bytes.Count).Trim();

            return text.Length == 0 ? null : text;
        }

        public override object Parse(string value)
        {
            return value;
        }

        public override string ToString(object value)
        {
            return (string)value;
        }
    }
}
