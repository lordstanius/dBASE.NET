using System;
using System.Globalization;
using System.Text;

namespace dBASE.NET.Encoders
{
    internal class IntegerEncoder : Encoder
    {
        public IntegerEncoder(Encoding encoding) : base(encoding) { }

        public override byte[] Encode(DbfField field, object data)
        {
            int value = 0;
            if (data != null) value = (int)data;
            return BitConverter.GetBytes(value);
        }

        public override object Decode(ArraySegment<byte> bytes, DbfMemo memo)
        {
            return BitConverter.ToInt32(bytes.Array, bytes.Offset);
        }

        public override object Parse(string value)
        {
            return int.Parse(value, CultureInfo.InvariantCulture);
        }
    }
}
